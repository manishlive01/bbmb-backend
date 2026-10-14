using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CEManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRManagements.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.CRSectionManagements.Entites;
using BBMB.BudgetService.Features.HierarchyManagement.Divisions.Entities;
using BBMB.BudgetService.Features.HierarchyManagement.DivisonSectionManagements.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Features.Offices.AccountOffices.Entities;
using BBMB.BudgetService.Features.Offices.RemittanceOffices.Entities;
using BBMB.BudgetService.Features.Pools.Entities;
using BBMB.BudgetService.Features.Projects.Entities;
using BBMB.BudgetService.Features.Regions.Entities;
using BBMB.BudgetService.Features.Units.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using Auth.Authorization.Requirements;
using BBMB.BudgetService.Shared.Messages;
using BBMB.BudgetService.Shared.Services.BudgetActors;
using BBMB.BudgetService.Shared.ResponseDTOs;
using Microsoft.AspNetCore.Authorization;
using Npgsql;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace BBMB.BudgetService.Features.BulkUploads
{
    public class BulkUploadHandler : IRequestHandler<BulkUploadCommand, Response>
    {
        /// <summary>
        /// Design §9.3: the right each upload type needs, checked before the sheet is read. Head and segment
        /// masters are the L-04 bulk load of F-01; hierarchy masters are office master loading. The
        /// "workmaster" type is removed (D-11): works enter only through the module (DDD T-23, L-08).
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> RightByType = new Dictionary<string, string>
        {
            ["majormaster"] = "F01.Create",
            ["submajormaster"] = "F01.Create",
            ["minormaster"] = "F01.Create",
            ["subminormaster"] = "F01.Create",
            ["detailmaster"] = "F01.Create",
            ["objectmaster"] = "F01.Create",
            ["headofaccountmaster"] = "F01.Create",
            ["cemaster"] = "Offices.Load",
            ["circlemaster"] = "Offices.Load",
            ["circlesectionmaster"] = "Offices.Load",
            ["divisionmaster"] = "Offices.Load",
            ["divisionsectionmaster"] = "Offices.Load",
        };

        private readonly BudgetContext _context;
        private readonly IAuthorizationService _authorization;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<BulkUploadHandler> _logger;
        private readonly IBudgetActorSource _actorSource;
        private readonly IBudgetActionScope _scope;
        private string _uploadType = string.Empty;

        /// <summary>Sheet row being saved, for "Row &lt;n&gt;: M-nn" (first row of the failing batch).</summary>
        private int _failingRow = 2;

        public BulkUploadHandler(
            BudgetContext context,
            IAuthorizationService authorization,
            IHttpContextAccessor httpContextAccessor,
            ILogger<BulkUploadHandler> logger,
            IBudgetActorSource actorSource,
            IBudgetActionScope scope)
        {
            _actorSource = actorSource;
            _scope = scope;
            _context = context;
            _authorization = authorization;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Translates a PostgreSQL rule failure through the same mapping as the global handler
        /// (<see cref="BudgetMessages.FromPostgres"/>) into "Row &lt;n&gt;: M-nn: &lt;catalogue text&gt;".
        /// Returns null for anything it does not map; the caller rethrows (design §9.3, §10).
        /// </summary>
        private Response? MapDatabaseFailure(Exception ex)
        {
            PostgresException? pg = null;
            for (var e = ex; e is not null && pg is null; e = e.InnerException)
                pg = e as PostgresException;
            if (pg is null)
                return null;
            var mapped = BudgetMessages.FromPostgres(pg);
            if (mapped is null)
                return null;

            _logger.LogWarning(
                "Bulk upload {UploadType} refused at row {Row}: {MessageId} (sqlstate {SqlState}, constraint {Constraint}, where {Where})",
                _uploadType, _failingRow, mapped.Value.MessageId, pg.SqlState, pg.ConstraintName, pg.Where);
            return new Response
            {
                IsSucced = false,
                MessageId = mapped.Value.MessageId,
                Message = $"Row {_failingRow}: {BudgetMessages.Format(mapped.Value.MessageId)}",
            };
        }

        public async Task<Response> Handle(
    BulkUploadCommand request,
    CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                return new Response
                {
                    IsSucced = false,
                    Message = "File is required"
                };

            if (string.IsNullOrWhiteSpace(request.uploadType))
                return new Response
                {
                    IsSucced = false,
                    Message = "UploadType is required"
                };

            var type = request.uploadType.Trim().ToLower();
            _uploadType = type;
            if (!RightByType.TryGetValue(type, out var right))
                return new Response
                {
                    IsSucced = false,
                    Message = "Invalid UploadType"
                };

            // Authorise by upload type before the sheet is read (design §9.3).
            var user = _httpContextAccessor.HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal();
            var authorised = await _authorization.AuthorizeAsync(user, null, BudgetRightRequirement.Policy(right));
            if (!authorised.Succeeded)
                return BudgetResponses.Fail(BudgetMessages.M20);

            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);
                ExcelPackage.License.SetNonCommercialPersonal("BBMB");
                ExcelPackage package;
                try
                {
                    package = new ExcelPackage(stream);
                    _ = package.Workbook.Worksheets.Count;
                }
                catch (Exception)
                {
                    // Not a readable workbook: the existing message, never the parser's exception text.
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Invalid Excel file"
                    };
                }
                using var ownedPackage = package;
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Invalid Excel file"
                    };

                var rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Excel has no data"
                    };

                // 🚀 NO TRANSACTION HERE NOW
                return type switch
                {
                    "majormaster" => await UploadMajorMaster(worksheet, rowCount, cancellationToken),
                    "submajormaster" => await UploadSubMajorMaster(worksheet, rowCount, cancellationToken),
                    "minormaster" => await UploadMinorMaster(worksheet, rowCount, cancellationToken),
                    "subminormaster" => await UploadSubMinorMaster(worksheet, rowCount, cancellationToken),
                    "detailmaster" => await UploadDetailMaster(worksheet, rowCount, cancellationToken),
                    "objectmaster" => await UploadObjectMaster(worksheet, rowCount, cancellationToken),
                    "headofaccountmaster" => await UploadHeadOfAccountMaster(worksheet, rowCount, cancellationToken),
                    "cemaster" => await UploadCEManagementMaster(worksheet, rowCount, cancellationToken),
                    "circlemaster" => await UploadCRManagementMaster(worksheet, rowCount, cancellationToken),
                    "circlesectionmaster" => await UploadCRSectionMaster(worksheet, rowCount, cancellationToken),
                    "divisionmaster" => await UploadDivisionMaster(worksheet, rowCount, cancellationToken),
                    "divisionsectionmaster" => await UploadDivSectionMaster(worksheet, rowCount, cancellationToken),
                    _ => new Response
                    {
                        IsSucced = false,
                        Message = "Invalid UploadType"
                    }
                };
            }
            catch (BulkUploadRowException ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: no exception text in the response; unmapped errors go to the global handler.
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // MAJOR HEAD
        // ============================================================

        private async Task<Response> UploadMajorMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var list = new List<MajorHead>();
                var excelDuplicateCheck = new HashSet<string>();

                var existingCodes = await _context.MajorHeads
                    .AsNoTracking()
                    .Select(x => x.MajorAccCode)
                    .ToListAsync(cancellationToken);

                var dbSet = new HashSet<string>(existingCodes);

                for (int row = 2; row <= rowCount; row++)
                {
                    var code = worksheet.Cells[row, 1].Text.Trim();
                    var desc = worksheet.Cells[row, 2].Text.Trim();

                    if (string.IsNullOrWhiteSpace(code) || code.Length != 4)
                        throw new BulkUploadRowException($"Invalid Major code at row {row}");

                    if (!excelDuplicateCheck.Add(code))
                        throw new BulkUploadRowException($"Duplicate Major code in Excel: {code}");

                    if (dbSet.Contains(code))
                        throw new BulkUploadRowException($"Major already exists: {code}");

                    list.Add(new MajorHead
                    {
                        MajorAccCode = code,
                        MajorAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                const int batchSize = 5000;

                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.MajorHeads.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                await transaction.CommitAsync(cancellationToken);

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} Major records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // SUB MAJOR HEAD
        // ============================================================
        private async Task<Response> UploadSubMajorMaster(
     ExcelWorksheet worksheet,
     int rowCount,
     CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var list = new List<SubMajorHead>();
                var excelDuplicateCheck = new HashSet<string>();

                // 🔥 Load existing Major codes (ONE DB HIT)
                var majorSet = new HashSet<string>(
                    await _context.MajorHeads
                        .AsNoTracking()
                        .Select(x => x.MajorAccCode)
                        .ToListAsync(cancellationToken));

                // 🔥 Load existing SubMajor composite keys (ONE DB HIT)
                var subMajorSet = new HashSet<(string Major, string SubMajor)>(
                    (await _context.SubMajorHeads
                        .AsNoTracking()
                        .Select(x => new { x.MajorAccCode, x.SubMajorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MajorAccCode, x.SubMajorAccCode)));

                for (int row = 2; row <= rowCount; row++)
                {
                    var major = worksheet.Cells[row, 1].Text?.Trim();
                    var subMajor = worksheet.Cells[row, 2].Text?.Trim();
                    var desc = worksheet.Cells[row, 3].Text?.Trim();

                    // 🔹 Required check
                    if (string.IsNullOrWhiteSpace(major) ||
                        string.IsNullOrWhiteSpace(subMajor) ||
                        string.IsNullOrWhiteSpace(desc))
                    {
                        throw new BulkUploadRowException($"Empty data at row {row}");
                    }

                    // 🔹 Length validation
                    if (major.Length != 4 || subMajor.Length != 2)
                    {
                        throw new BulkUploadRowException(
                            $"Invalid format at row {row}. Major must be 4 & SubMajor must be 2 characters.");
                    }

                    var compositeKey = (major, subMajor);
                    var excelKey = $"{major}-{subMajor}";

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(excelKey))
                    {
                        throw new BulkUploadRowException($"Duplicate SubMajor in Excel: {excelKey}");
                    }

                    // 🔹 FK check
                    if (!majorSet.Contains(major))
                    {
                        throw new BulkUploadRowException($"Major does not exist: {major}");
                    }

                    // 🔹 DB duplicate check
                    if (subMajorSet.Contains(compositeKey))
                    {
                        throw new BulkUploadRowException($"SubMajor already exists: {excelKey}");
                    }

                    list.Add(new SubMajorHead
                    {
                        MajorAccCode = major,
                        SubMajorAccCode = subMajor,
                        SubMajorAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.SubMajorHeads.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} SubMajor records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // MINOR HEAD
        // ============================================================
        private async Task<Response> UploadMinorMaster(
      ExcelWorksheet worksheet,
      int rowCount,
      CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var list = new List<MinorHead>();
                var excelDuplicateCheck = new HashSet<string>();

                // 🔥 ONE DB HIT
                var existingSet = new HashSet<string>(
                    await _context.MinorHeads
                        .AsNoTracking()
                        .Select(x => x.MinorAccCode)
                        .ToListAsync(cancellationToken));

                for (int row = 2; row <= rowCount; row++)
                {
                    var code = worksheet.Cells[row, 1].Text?.Trim();
                    var desc = worksheet.Cells[row, 2].Text?.Trim();

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(code) ||
                        string.IsNullOrWhiteSpace(desc))
                    {
                        throw new BulkUploadRowException($"Empty Minor at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new BulkUploadRowException(
                            $"Invalid Minor at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new BulkUploadRowException($"Duplicate Minor in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new BulkUploadRowException($"Minor already exists: {code}");
                    }

                    list.Add(new MinorHead
                    {
                        MinorAccCode = code,
                        MinorAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.MinorHeads.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                await transaction.CommitAsync(cancellationToken);

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} Minor records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }
        // ============================================================
        // SUB MINOR HEAD
        // ============================================================
        private async Task<Response> UploadSubMinorMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var list = new List<SubMinorHead>();
                var excelDuplicateCheck = new HashSet<(string Minor, string SubMinor)>();

                // 🔥 ONE DB HIT - Load existing Minor codes
                var minorSet = new HashSet<string>(
                    await _context.MinorHeads
                        .AsNoTracking()
                        .Select(x => x.MinorAccCode)
                        .ToListAsync(cancellationToken));

                // 🔥 ONE DB HIT - Load existing SubMinor composite keys
                var subMinorSet = new HashSet<(string Minor, string SubMinor)>(
                    (await _context.SubMinorHeads
                        .AsNoTracking()
                        .Select(x => new { x.MinorAccCode, x.SubMinorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MinorAccCode, x.SubMinorAccCode)));

                for (int row = 2; row <= rowCount; row++)
                {
                    var minor = worksheet.Cells[row, 1].Text?.Trim();
                    var subMinor = worksheet.Cells[row, 2].Text?.Trim();
                    var desc = worksheet.Cells[row, 3].Text?.Trim();

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(minor) ||
                        string.IsNullOrWhiteSpace(subMinor) ||
                        string.IsNullOrWhiteSpace(desc))
                    {
                        throw new BulkUploadRowException($"Empty SubMinor at row {row}");
                    }

                    // 🔹 Format validation
                    if (minor.Length != 3 || !minor.All(char.IsDigit) ||
                        subMinor.Length != 2 || !subMinor.All(char.IsDigit))
                    {
                        throw new BulkUploadRowException(
                            $"Invalid format at row {row}. Minor must be 3 digit & SubMinor must be 2 digit.");
                    }

                    var compositeKey = (minor, subMinor);

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(compositeKey))
                    {
                        throw new BulkUploadRowException($"Duplicate SubMinor in Excel: {minor}-{subMinor}");
                    }

                    // 🔹 FK check
                    if (!minorSet.Contains(minor))
                    {
                        throw new BulkUploadRowException($"Minor does not exist: {minor}");
                    }

                    // 🔹 DB duplicate check
                    if (subMinorSet.Contains(compositeKey))
                    {
                        throw new BulkUploadRowException($"SubMinor already exists: {minor}-{subMinor}");
                    }

                    list.Add(new SubMinorHead
                    {
                        MinorAccCode = minor,
                        SubMinorAccCode = subMinor,
                        SubMinorAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.SubMinorHeads.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} SubMinor records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // DETAIL HEAD
        // ============================================================
        private async Task<Response> UploadDetailMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var list = new List<DetailHead>();
                var excelDuplicateCheck = new HashSet<string>();

                // 🔥 ONE DB HIT – Load existing Detail codes
                var existingSet = new HashSet<string>(
                    await _context.DetailHeads
                        .AsNoTracking()
                        .Select(x => x.DetailAccCode)
                        .ToListAsync(cancellationToken));

                for (int row = 2; row <= rowCount; row++)
                {
                    var code = worksheet.Cells[row, 1].Text?.Trim();
                    var desc = worksheet.Cells[row, 2].Text?.Trim();

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(code) ||
                        string.IsNullOrWhiteSpace(desc))
                    {
                        throw new BulkUploadRowException($"Empty Detail at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new BulkUploadRowException(
                            $"Invalid Detail code at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new BulkUploadRowException($"Duplicate Detail in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new BulkUploadRowException($"Detail already exists: {code}");
                    }

                    list.Add(new DetailHead
                    {
                        DetailAccCode = code,
                        DetailAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.DetailHeads.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} Detail records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // OBJECT CLASSIFICATION
        // ============================================================
        private async Task<Response> UploadObjectMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var list = new List<ObjectClassification>();
                var excelDuplicateCheck = new HashSet<string>();

                // 🔥 ONE DB HIT – Load existing object codes
                var existingSet = new HashSet<string>(
                    await _context.ObjectClassifications
                        .AsNoTracking()
                        .Select(x => x.ObjectAccCode)
                        .ToListAsync(cancellationToken));

                for (int row = 2; row <= rowCount; row++)
                {
                    var code = worksheet.Cells[row, 1].Text?.Trim();
                    var desc = worksheet.Cells[row, 2].Text?.Trim();

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(code) ||
                        string.IsNullOrWhiteSpace(desc))
                    {
                        throw new BulkUploadRowException(
                            $"Object code or description is empty at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new BulkUploadRowException(
                            $"Invalid Object code at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new BulkUploadRowException(
                            $"Duplicate Object code in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new BulkUploadRowException(
                            $"Object already exists in database: {code}");
                    }

                    list.Add(new ObjectClassification
                    {
                        ObjectAccCode = code,
                        ObjectAccDesc = desc,
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.ObjectClassifications.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} Object records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // HEAD OF ACCOUNT MASTER
        // ============================================================
        private async Task<Response> UploadHeadOfAccountMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            // T-07 carries fn_require_actor from P1 (DB-28): the action scope REPLACES the arm's own
            // transaction (a second BeginTransactionAsync would throw); every batch and the commit stay in it.
            if (await _actorSource.GetAsync(cancellationToken) is null)
                return BudgetResponses.Fail(BudgetMessages.M20);
            await using var transaction =
                await _scope.BeginAsync("BULK_UPLOAD:headofaccountmaster", ct: cancellationToken);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var list = new List<HeadOfAccount>();
                var excelDuplicateCheck = new HashSet<string>();

                // 🔥 Preload master data (single DB hits)

                var majors = new HashSet<string>(
                    await _context.MajorHeads.AsNoTracking()
                        .Select(x => x.MajorAccCode)
                        .ToListAsync(cancellationToken));

                var subMajors = new HashSet<(string Major, string SubMajor)>(
                    (await _context.SubMajorHeads.AsNoTracking()
                        .Select(x => new { x.MajorAccCode, x.SubMajorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MajorAccCode, x.SubMajorAccCode)));

                var minors = new HashSet<string>(
                    await _context.MinorHeads.AsNoTracking()
                        .Select(x => x.MinorAccCode)
                        .ToListAsync(cancellationToken));

                var subMinors = new HashSet<(string Minor, string SubMinor)>(
                    (await _context.SubMinorHeads.AsNoTracking()
                        .Select(x => new { x.MinorAccCode, x.SubMinorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MinorAccCode, x.SubMinorAccCode)));

                var details = new HashSet<string>(
                    await _context.DetailHeads.AsNoTracking()
                        .Select(x => x.DetailAccCode)
                        .ToListAsync(cancellationToken));

                var objects = new HashSet<string>(
                    await _context.ObjectClassifications.AsNoTracking()
                        .Select(x => x.ObjectAccCode)
                        .ToListAsync(cancellationToken));

                var existingHoaSet = new HashSet<string>(
                    await _context.HeadOfAccounts.AsNoTracking()
                        .Select(x =>
                            x.MajorAccCode +
                            x.SubMajorAccCode +
                            x.MinorAccCode +
                            x.SubMinorAccCode +
                            x.DetailAccCode +
                            x.ObjectAccCode)
                        .ToListAsync(cancellationToken));

                for (int row = 2; row <= rowCount; row++)
                {
                    var fullCode = worksheet.Cells[row, 1].Text?.Trim();
                    var desc = worksheet.Cells[row, 2].Text?.Trim();

                    if (string.IsNullOrWhiteSpace(fullCode) ||
                        string.IsNullOrWhiteSpace(desc))
                        throw new BulkUploadRowException($"Empty HeadOfAccount at row {row}");

                    if (fullCode.Length != 17 || !fullCode.All(char.IsDigit))
                        throw new BulkUploadRowException($"Invalid 17 digit code at row {row}");

                    if (!excelDuplicateCheck.Add(fullCode))
                        throw new BulkUploadRowException($"Duplicate in Excel: {fullCode}");

                    if (existingHoaSet.Contains(fullCode))
                        throw new BulkUploadRowException($"HeadOfAccount already exists: {fullCode}");

                    var major = fullCode.Substring(0, 4);
                    var subMajor = fullCode.Substring(4, 2);
                    var minor = fullCode.Substring(6, 3);
                    var subMinor = fullCode.Substring(9, 2);
                    var detail = fullCode.Substring(11, 3);
                    var obj = fullCode.Substring(14, 3);

                    if (!majors.Contains(major))
                        throw new BulkUploadRowException($"Major missing: {major}");

                    if (!subMajors.Contains((major, subMajor)))
                        throw new BulkUploadRowException($"SubMajor missing: {major}{subMajor}");

                    if (!minors.Contains(minor))
                        throw new BulkUploadRowException($"Minor missing: {minor}");

                    if (!subMinors.Contains((minor, subMinor)))
                        throw new BulkUploadRowException($"SubMinor missing: {minor}{subMinor}");

                    if (!details.Contains(detail))
                        throw new BulkUploadRowException($"Detail missing: {detail}");

                    if (!objects.Contains(obj))
                        throw new BulkUploadRowException($"Object missing: {obj}");

                    list.Add(new HeadOfAccount
                    {
                        MajorAccCode = major,
                        SubMajorAccCode = subMajor,
                        MinorAccCode = minor,
                        SubMinorAccCode = subMinor,
                        DetailAccCode = detail,
                        ObjectAccCode = obj,
                        HeadDesc = desc,
                        // L-04 path (design §9.3): legacy origin; the flag by the BUD-267 rule.
                        Origin = HeadOfAccountOrigin.Legacy,
                        PartyEmpRequired = HeadOfAccountOrigin.PartyEmpRequiredFor(major),
                        PartyLedgToPrint = "N",
                        TdsDeducted = "N",
                        AdvanceHead = "N",
                        BankCash = "N",
                        MonthlyCarryFwd = "N",
                        ModificationDate = DateTime.UtcNow,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 Batch Insert
                const int batchSize = 5000;

                for (int i = 0; i < list.Count; i += batchSize)
                {

                    _failingRow = i + 2; // first sheet row of this batch (rows map 1:1 from row 2)
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.HeadOfAccounts.AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} HeadOfAccount records uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // CE MANAGEMENT MASTER
        // ============================================================
        private async Task<Response> UploadCEManagementMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            try
            {
                // 🔥 STEP 1: Existing CE Codes
                var existingSet = new HashSet<int>(
                    await _context.Set<CEManagement>()
                        .AsNoTracking()
                        .Select(x => x.CECode)
                        .ToListAsync(cancellationToken));

                // 🔥 STEP 2: Project Codes
                var projectSet = new HashSet<string>(
                    await _context.Set<Project>()
                        .AsNoTracking()
                        .Select(x => x.ProjCode)
                        .ToListAsync(cancellationToken));

                var duplicateExcelCheck = new HashSet<int>();

                // 🔥 STEP 3: DB connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                // 🔥 STEP 4: COPY (fix "desc")
                using var writer = conn.BeginBinaryImport(@"
            COPY budget.ce_management
            (ce_code, name, project_code, ""desc"", location, date, status, user_code)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: STREAM processing
                for (int row = 2; row <= rowCount; row++)
                {
                    var ceCodeValue = worksheet.Cells[row, 1].Value;
                    var name = worksheet.Cells[row, 2].Text?.Trim();
                    var projectCode = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 CECode validation
                    if (ceCodeValue == null || !int.TryParse(ceCodeValue.ToString(), out int ceCode))
                        throw new BulkUploadRowException($"Invalid CECode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(projectCode))
                        throw new BulkUploadRowException($"Required fields missing at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(ceCode))
                        throw new BulkUploadRowException($"Duplicate CECode in Excel: {ceCode}");

                    // 🔹 DB duplicate
                    if (existingSet.Contains(ceCode))
                        throw new BulkUploadRowException($"CECode already exists: {ceCode}");

                    // 🔹 FK validation
                    if (!projectSet.Contains(projectCode))
                        throw new BulkUploadRowException($"ProjectCode not found: {projectCode}");

                    // 🔥 WRITE ROW
                    writer.StartRow();
                    writer.Write(ceCode);
                    writer.Write(name);
                    writer.Write(projectCode);
                    writer.Write(string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                    writer.Write(string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location);
                    writer.Write(DateTime.UtcNow);
                    writer.Write(true);
                    writer.Write("SYSTEM");

                    successCount++;
                }

                // 🔥 FINAL COMMIT (handled by PostgreSQL COPY)
                await writer.CompleteAsync(cancellationToken);
                await conn.CloseAsync();

                return new Response
                {
                    IsSucced = true,
                    Message = $"{successCount} CEManagement records uploaded successfully."
                };
            }
            catch (BulkUploadRowException ex)
            {
                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // CR MANAGEMENT MASTER
        // ============================================================

        private async Task<Response> UploadCRManagementMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            try
            {
                // 🔥 STEP 1: CECode → CEId mapping
                var ceDictionary = await _context.Set<CEManagement>()
                    .AsNoTracking()
                    .Select(x => new { x.CECode, x.Id })
                    .ToDictionaryAsync(x => x.CECode, x => x.Id, cancellationToken);

                // 🔥 STEP 2: Existing CR Codes
                var existingCRSet = new HashSet<int>(
                    await _context.Set<CRManagement>()
                        .AsNoTracking()
                        .Select(x => x.CRCode)
                        .ToListAsync(cancellationToken));

                var duplicateExcelCheck = new HashSet<int>();

                // 🔥 STEP 3: DB Connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                // 🔥 STEP 4: COPY (FIXED "desc")
                using var writer = conn.BeginBinaryImport(@"
            COPY budget.cr_management
            (cr_code, ce_id, name, ""desc"", location, date, status, user_code)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: STREAM processing
                for (int row = 2; row <= rowCount; row++)
                {
                    var ceCodeValue = worksheet.Cells[row, 1].Value;
                    var crCodeValue = worksheet.Cells[row, 2].Value;

                    var name = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 CECode validation
                    if (ceCodeValue == null || !int.TryParse(ceCodeValue.ToString(), out int ceCode))
                        throw new BulkUploadRowException($"Invalid CECode at row {row}");

                    // 🔹 CRCode validation
                    if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out int crCode))
                        throw new BulkUploadRowException($"Invalid CRCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new BulkUploadRowException($"Name is required at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(crCode))
                        throw new BulkUploadRowException($"Duplicate CRCode in Excel: {crCode}");

                    // 🔹 DB duplicate
                    if (existingCRSet.Contains(crCode))
                        throw new BulkUploadRowException($"CRCode already exists: {crCode}");

                    // 🔹 FK mapping (CECode → CEId)
                    if (!ceDictionary.TryGetValue(ceCode, out int ceId))
                        throw new BulkUploadRowException($"CECode not found: {ceCode}");

                    // 🔥 WRITE ROW (FASTEST)
                    writer.StartRow();
                    writer.Write(crCode);
                    writer.Write(ceId);
                    writer.Write(name);
                    writer.Write(string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                    writer.Write(string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location);
                    writer.Write(DateTime.UtcNow);
                    writer.Write(true);
                    writer.Write("SYSTEM");

                    successCount++;
                }

                // 🔥 FINAL COMMIT
                await writer.CompleteAsync(cancellationToken);
                await conn.CloseAsync();

                return new Response
                {
                    IsSucced = true,
                    Message = $"{successCount} CRManagement records uploaded successfully (BULK)"
                };
            }
            catch (BulkUploadRowException ex)
            {
                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }

        // ============================================================
        // CR Section MASTER
        // ============================================================

        private async Task<Response> UploadCRSectionMaster(
     ExcelWorksheet worksheet,
     int rowCount,
     CancellationToken cancellationToken)
        {
            try
            {
                // 🔥 STEP 1: CRCode → CRId mapping
                var crDictionary = await _context.Set<CRManagement>()
                    .AsNoTracking()
                    .Select(x => new { x.CRCode, x.Id })
                    .ToDictionaryAsync(x => x.CRCode, x => x.Id, cancellationToken);

                // 🔥 STEP 2: Existing DB composite keys (CRId + CRSectionCode)
                var existingCRSectionSet = new HashSet<(int CRId, int CRSectionCode)>(
                    await _context.Set<CRSection>()
                        .AsNoTracking()
                        .Select(x => new ValueTuple<int, int>(x.CRId, x.CRSectionCode))
                        .ToListAsync(cancellationToken));

                // 🔥 STEP 3: Excel duplicate check (composite)
                var duplicateExcelCheck = new HashSet<(int CRId, int CRSectionCode)>();

                // 🔥 STEP 4: DB Connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                using var writer = conn.BeginBinaryImport(@"
            COPY budget.cr_section
            (cr_id, cr_section_code, name, ""desc"", location, date, status)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: Process Excel rows
                for (int row = 2; row <= rowCount; row++)
                {
                    var crCodeValue = worksheet.Cells[row, 1].Value;
                    var crSectionCodeValue = worksheet.Cells[row, 2].Value;

                    var name = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 Validate CRCode
                    if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out int crCode))
                        throw new BulkUploadRowException($"Invalid CRCode at row {row}");

                    // 🔹 Map CRId FIRST (⚠️ important)
                    if (!crDictionary.TryGetValue(crCode, out int crId))
                        throw new BulkUploadRowException($"CRCode not found: {crCode} at row {row}");

                    // 🔹 Validate CRSectionCode
                    if (crSectionCodeValue == null || !int.TryParse(crSectionCodeValue.ToString(), out int crSectionCode))
                        throw new BulkUploadRowException($"Invalid CRSectionCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new BulkUploadRowException($"Name is required at row {row}");

                    // 🔹 Create composite key
                    var key = (crId, crSectionCode);

                    // 🔹 Excel duplicate check
                    if (!duplicateExcelCheck.Add(key))
                        throw new BulkUploadRowException($"Duplicate CRId + CRSectionCode in Excel: {crId}-{crSectionCode} (row {row})");

                    // 🔹 DB duplicate check
                    if (existingCRSectionSet.Contains(key))
                        throw new BulkUploadRowException($"CRSection already exists for CRId: {crId} and Code: {crSectionCode} (row {row})");

                    // 🔥 WRITE ROW
                    writer.StartRow();
                    writer.Write(crId);
                    writer.Write(crSectionCode);
                    writer.Write(name);
                    writer.Write(string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                    writer.Write(string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location);
                    writer.Write(DateTime.UtcNow);
                    writer.Write(true);

                    successCount++;
                }

                // 🔥 FINAL COMMIT
                await writer.CompleteAsync(cancellationToken);
                await conn.CloseAsync();

                return new Response
                {
                    IsSucced = true,
                    Message = $"{successCount} CRSection records uploaded successfully (BULK)"
                };
            }
            catch (BulkUploadRowException ex)
            {
                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }


        // ============================================================
        // DIVISION MASTER
        // ============================================================

        private async Task<Response> UploadDivisionMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            try
            {
                var divisionSet = new HashSet<int>(
                    await _context.Set<Division>()
                        .AsNoTracking()
                        .Select(x => x.DivisionCode)
                        .ToListAsync(cancellationToken));

                var crDict = await _context.Set<CRManagement>()
                    .AsNoTracking()
                    .Select(x => new { x.CRCode, x.Id })
                    .ToDictionaryAsync(x => x.CRCode, x => x.Id, cancellationToken);

                var majorSet = new HashSet<string>(
                    await _context.Set<MajorHead>().AsNoTracking().Select(x => x.MajorAccCode).ToListAsync(cancellationToken));

                var accOfficeSet = new HashSet<string>(
                    await _context.Set<AccountOffice>().AsNoTracking().Select(x => x.AccOffice).ToListAsync(cancellationToken));

                var unitSet = new HashSet<string>(
                    await _context.Set<UnitMaster>().AsNoTracking().Select(x => x.UnitCode).ToListAsync(cancellationToken));

                var poolSet = new HashSet<string>(
                    await _context.Set<Pool>().AsNoTracking().Select(x => x.PoolCode).ToListAsync(cancellationToken));

                var regionSet = new HashSet<string>(
                    await _context.Set<Region>().AsNoTracking().Select(x => x.RegionCode).ToListAsync(cancellationToken));

                var excelDuplicateCheck = new HashSet<int>();

                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                using var writer = conn.BeginBinaryImport(@"
        COPY budget.division
        (division_code, cr_id, division_desc,
         ctrl_office1, ctrl_office2,
         ro_code, major_acc_code, acc_office,
         unit_code, pool_code, umd_unit,
         region_code, ddo_tds,
         parent_division_code, modification_date, user_code)
        FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                for (int row = 2; row <= rowCount; row++)
                {
                    // ✅ UPDATED INDEXES
                    var divisionCode = int.Parse(worksheet.Cells[row, 1].Text);
                    var divisionDesc = worksheet.Cells[row, 2].Text?.Trim();
                    var crCode = int.Parse(worksheet.Cells[row, 3].Text);

                    var ctrl1 = worksheet.Cells[row, 4].Text;
                    var ctrl2 = worksheet.Cells[row, 5].Text;
                    var roCode = worksheet.Cells[row, 6].Text;
                    var major = worksheet.Cells[row, 7].Text;
                    var acc = worksheet.Cells[row, 8].Text;
                    var unit = worksheet.Cells[row, 9].Text;
                    var pool = worksheet.Cells[row, 10].Text;
                    var umd = worksheet.Cells[row, 11].Text;
                    var region = worksheet.Cells[row, 12].Text;
                    var ddo = worksheet.Cells[row, 13].Text;
                    var parentText = worksheet.Cells[row, 14].Text;

                    int? parent = string.IsNullOrWhiteSpace(parentText) ? null : int.Parse(parentText);

                    if (!excelDuplicateCheck.Add(divisionCode))
                        throw new BulkUploadRowException($"Duplicate DivisionCode {divisionCode}");

                    if (divisionSet.Contains(divisionCode))
                        throw new BulkUploadRowException($"DivisionCode exists {divisionCode}");

                    if (!crDict.TryGetValue(crCode, out int crId))
                        throw new BulkUploadRowException($"CRCode not found {crCode}");

                    if (!string.IsNullOrWhiteSpace(major) && !majorSet.Contains(major))
                        throw new BulkUploadRowException($"MajorAccCode not found {major}");

                    if (!string.IsNullOrWhiteSpace(acc) && !accOfficeSet.Contains(acc))
                        throw new BulkUploadRowException($"AccOffice not found {acc}");

                    if (!string.IsNullOrWhiteSpace(unit) && !unitSet.Contains(unit))
                        throw new BulkUploadRowException($"UnitCode not found {unit}");

                    if (!string.IsNullOrWhiteSpace(region) && !regionSet.Contains(region))
                        throw new BulkUploadRowException($"RegionCode not found {region}");

                    // 🔥 WRITE
                    writer.StartRow();
                    writer.Write(divisionCode);
                    writer.Write(crId);
                    writer.Write(divisionDesc);
                    writer.Write(string.IsNullOrEmpty(ctrl1) ? (object)DBNull.Value : int.Parse(ctrl1));
                    writer.Write(string.IsNullOrEmpty(ctrl2) ? (object)DBNull.Value : int.Parse(ctrl2));
                    writer.Write(roCode ?? (object)DBNull.Value);
                    writer.Write(major ?? (object)DBNull.Value);
                    writer.Write(acc ?? (object)DBNull.Value);
                    writer.Write(unit ?? (object)DBNull.Value);
                    writer.Write(pool ?? (object)DBNull.Value);
                    writer.Write(umd ?? (object)DBNull.Value);
                    writer.Write(region ?? (object)DBNull.Value);
                    writer.Write(ddo ?? (object)DBNull.Value);
                    writer.Write(parent ?? (object)DBNull.Value);
                    writer.Write(DateTime.UtcNow);
                    writer.Write("SYSTEM");

                    successCount++;
                }

                await writer.CompleteAsync(cancellationToken);
                await conn.CloseAsync();

                return new Response
                {
                    IsSucced = true,
                    Message = $"{successCount} Division uploaded successfully"
                };
            }
            catch (BulkUploadRowException ex)
            {
                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }
        private async Task<Response> UploadDivSectionMaster(
      ExcelWorksheet worksheet,
      int rowCount,
      CancellationToken cancellationToken)
        {
            try
            {
                // 🔥 STEP 1: DivisionCode → DivisionId mapping
                var divisionDict = await _context.Set<Division>()
                    .AsNoTracking()
                    .Select(x => new { x.DivisionCode, x.Id })
                    .ToDictionaryAsync(x => x.DivisionCode, x => x.Id, cancellationToken);

                // 🔥 STEP 2: Existing composite keys (DivisionId + DivSectionCode)
                var existingSet = new HashSet<(int DivisionId, int DivSectionCode)>(
                    await _context.Set<DivSection>()
                        .AsNoTracking()
                        .Select(x => new ValueTuple<int, int>(x.DivisionId, x.DivSectionCode))
                        .ToListAsync(cancellationToken));

                // 🔥 STEP 3: Excel duplicate check (composite)
                var duplicateExcelCheck = new HashSet<(int DivisionId, int DivSectionCode)>();

                // 🔥 STEP 4: DB Connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                using var writer = conn.BeginBinaryImport(@"
            COPY budget.div_section
            (division_id, div_section_code, name, ""desc"", location, date, status)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: Process rows
                for (int row = 2; row <= rowCount; row++)
                {
                    var divisionCodeValue = worksheet.Cells[row, 1].Value;
                    var divSectionCodeValue = worksheet.Cells[row, 2].Value;

                    var name = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 Validate DivisionCode
                    if (divisionCodeValue == null || !int.TryParse(divisionCodeValue.ToString(), out int divisionCode))
                        throw new BulkUploadRowException($"Invalid DivisionCode at row {row}");

                    // 🔹 Map DivisionId FIRST
                    if (!divisionDict.TryGetValue(divisionCode, out int divisionId))
                        throw new BulkUploadRowException($"DivisionCode not found: {divisionCode} at row {row}");

                    // 🔹 Validate DivSectionCode
                    if (divSectionCodeValue == null || !int.TryParse(divSectionCodeValue.ToString(), out int divSectionCode))
                        throw new BulkUploadRowException($"Invalid DivSectionCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new BulkUploadRowException($"Name is required at row {row}");

                    // 🔹 Composite key
                    var key = (divisionId, divSectionCode);

                    // 🔹 Excel duplicate check
                    if (!duplicateExcelCheck.Add(key))
                        throw new BulkUploadRowException($"Duplicate DivisionId + DivSectionCode in Excel: {divisionId}-{divSectionCode} (row {row})");

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(key))
                        throw new BulkUploadRowException($"DivSection already exists for DivisionId: {divisionId} and Code: {divSectionCode} (row {row})");

                    // 🔥 WRITE ROW
                    writer.StartRow();
                    writer.Write(divisionId);
                    writer.Write(divSectionCode);
                    writer.Write(name);
                    writer.Write(string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                    writer.Write(string.IsNullOrWhiteSpace(location) ? (object)DBNull.Value : location);
                    writer.Write(DateTime.UtcNow);
                    writer.Write(true);

                    successCount++;
                }

                // 🔥 FINAL COMMIT
                await writer.CompleteAsync(cancellationToken);
                await conn.CloseAsync();

                return new Response
                {
                    IsSucced = true,
                    Message = $"{successCount} DivSection records uploaded successfully (BULK)"
                };
            }
            catch (BulkUploadRowException ex)
            {
                // App-authored sheet validation message (row, field); never exception text from the database.
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                // BUD-012, BUD-261: database text never reaches the response (design §9.3).
                var mapped = MapDatabaseFailure(ex);
                if (mapped is null)
                    throw;
                return mapped;
            }
        }
        //    private async Task<Response> UploadDivisionMaster(
        //ExcelWorksheet worksheet,
        //int rowCount,
        //CancellationToken cancellationToken)
        //    {
        //        await using var transaction =
        //            await _context.Database.BeginTransactionAsync(cancellationToken);

        //        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        //        try
        //        {
        //            var list = new List<Division>();
        //            var excelDuplicateCheck = new HashSet<int>();

        //            // 🔥 ONE TIME DB LOADS
        //            var divisionSet = new HashSet<int>(
        //                await _context.Set<Division>()
        //                    .AsNoTracking()
        //                    .Select(x => x.DivisionCode)
        //                    .ToListAsync(cancellationToken));

        //            var ceSet = new HashSet<int>(
        //                await _context.Set<CEManagement>()
        //                    .AsNoTracking()
        //                    .Select(x => x.CECode)
        //                    .ToListAsync(cancellationToken));

        //            var crSet = new HashSet<int>(
        //                await _context.Set<CRManagement>()
        //                    .AsNoTracking()
        //                    .Select(x => x.CRCode)
        //                    .ToListAsync(cancellationToken));

        //            var majorSet = new HashSet<string>(
        //                await _context.Set<MajorHead>()
        //                    .AsNoTracking()
        //                    .Select(x => x.MajorAccCode)
        //                    .ToListAsync(cancellationToken));

        //            var roSet = new HashSet<string>(
        //                await _context.Set<RemittanceOffice>()
        //                    .AsNoTracking()
        //                    .Select(x => x.RoCode)
        //                    .ToListAsync(cancellationToken));

        //            var accOfficeSet = new HashSet<string>(
        //                await _context.Set<AccountOffice>()
        //                    .AsNoTracking()
        //                    .Select(x => x.AccOffice)
        //                    .ToListAsync(cancellationToken));

        //            var unitSet = new HashSet<string>(
        //                await _context.Set<UnitMaster>()
        //                    .AsNoTracking()
        //                    .Select(x => x.UnitCode)
        //                    .ToListAsync(cancellationToken));

        //            var poolSet = new HashSet<string>(
        //                await _context.Set<Pool>()
        //                    .AsNoTracking()
        //                    .Select(x => x.PoolCode)
        //                    .ToListAsync(cancellationToken));

        //            var regionSet = new HashSet<string>(
        //                await _context.Set<Region>()
        //                    .AsNoTracking()
        //                    .Select(x => x.RegionCode)
        //                    .ToListAsync(cancellationToken));

        //            for (int row = 2; row <= rowCount; row++)
        //            {
        //                var divisionCodeText = worksheet.Cells[row, 1].Text?.Trim();
        //                var divSectionText = worksheet.Cells[row, 2].Text?.Trim();
        //                var divisionDesc = worksheet.Cells[row, 3].Text?.Trim();

        //                var ceCodeValue = worksheet.Cells[row, 4].Value;
        //                var crCodeValue = worksheet.Cells[row, 5].Value;

        //                var majorAccCode = worksheet.Cells[row, 6].Text?.Trim();
        //                var roCode = worksheet.Cells[row, 7].Text?.Trim();
        //                var accOffice = worksheet.Cells[row, 8].Text?.Trim();
        //                var unitCode = worksheet.Cells[row, 9].Text?.Trim();
        //                var poolCode = worksheet.Cells[row, 10].Text?.Trim();
        //                var regionCode = worksheet.Cells[row, 11].Text?.Trim();
        //                var parentDivisionText = worksheet.Cells[row, 12].Text?.Trim();

        //                // 🔹 Required validation
        //                if (string.IsNullOrWhiteSpace(divisionCodeText) ||
        //                    string.IsNullOrWhiteSpace(divSectionText) ||
        //                    string.IsNullOrWhiteSpace(divisionDesc))
        //                {
        //                    throw new BulkUploadRowException($"Required fields missing at row {row}");
        //                }

        //                if (!int.TryParse(divisionCodeText, out var divisionCode))
        //                    throw new BulkUploadRowException($"Invalid DivisionCode '{divisionCodeText}' at row {row}");

        //                if (!int.TryParse(divSectionText, out var divSection))
        //                    throw new BulkUploadRowException($"Invalid DivSection '{divSectionText}' at row {row}");

        //                if (ceCodeValue == null || !int.TryParse(ceCodeValue.ToString(), out var ceCode))
        //                    throw new BulkUploadRowException($"Invalid CECode at row {row}");

        //                if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out var crCode))
        //                    throw new BulkUploadRowException($"Invalid CRCode at row {row}");

        //                int? parentDivision = null;
        //                if (!string.IsNullOrWhiteSpace(parentDivisionText))
        //                {
        //                    if (!int.TryParse(parentDivisionText, out var parent))
        //                        throw new BulkUploadRowException($"Invalid ParentDivisionCode '{parentDivisionText}' at row {row}");

        //                    parentDivision = parent;
        //                }

        //                // 🔹 Excel duplicate
        //                if (!excelDuplicateCheck.Add(divisionCode))
        //                    throw new BulkUploadRowException($"Duplicate DivisionCode '{divisionCode}' in Excel (row {row})");

        //                // 🔹 DB duplicate
        //                if (divisionSet.Contains(divisionCode))
        //                    throw new BulkUploadRowException($"DivisionCode '{divisionCode}' already exists in database (row {row})");

        //                // 🔹 FK validations
        //                if (!ceSet.Contains(ceCode))
        //                    throw new BulkUploadRowException($"CECode '{ceCode}' not found (row {row})");

        //                if (!crSet.Contains(crCode))
        //                    throw new BulkUploadRowException($"CRCode '{crCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(majorAccCode) && !majorSet.Contains(majorAccCode))
        //                    throw new BulkUploadRowException($"MajorAccCode '{majorAccCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(roCode) && !roSet.Contains(roCode))
        //                    throw new BulkUploadRowException($"RoCode '{roCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(accOffice) && !accOfficeSet.Contains(accOffice))
        //                    throw new BulkUploadRowException($"AccOffice '{accOffice}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(unitCode) && !unitSet.Contains(unitCode))
        //                    throw new BulkUploadRowException($"UnitCode '{unitCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(poolCode) && !poolSet.Contains(poolCode))
        //                    throw new BulkUploadRowException($"PoolCode '{poolCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(regionCode) && !regionSet.Contains(regionCode))
        //                    throw new BulkUploadRowException($"RegionCode '{regionCode}' not found (row {row})");

        //                list.Add(new Division
        //                {
        //                    DivisionCode = divisionCode,
        //                    //DivSection = divSection,
        //                    DivisionDesc = divisionDesc,
        //                    //CECode = ceCode,
        //                    //CRCode = crCode,
        //                    MajorAccCode = majorAccCode,
        //                    RoCode = roCode,
        //                    AccOffice = accOffice,
        //                    UnitCode = unitCode,
        //                    PoolCode = poolCode,
        //                    RegionCode = regionCode,
        //                    ParentDivisionCode = parentDivision,
        //                    ModificationDate = DateTime.UtcNow,
        //                    UserCode = "SYSTEM"
        //                });
        //            }

        //            // 🔥 Batch Insert
        //            const int batchSize = 5000;

        //            for (int i = 0; i < list.Count; i += batchSize)
        //            {
        //                var batch = list.Skip(i).Take(batchSize).ToList();

        //                await _context.Set<Division>().AddRangeAsync(batch, cancellationToken);
        //                await _context.SaveChangesAsync(cancellationToken);

        //                _context.ChangeTracker.Clear();
        //            }

        //            await transaction.CommitAsync(cancellationToken);

        //            _context.ChangeTracker.AutoDetectChangesEnabled = true;

        //            return new Response
        //            {
        //                IsSucced = true,
        //                Message = $"{list.Count} Division records uploaded successfully"
        //            };
        //        }
        //        catch (Exception ex)
        //        {
        //            await transaction.RollbackAsync(cancellationToken);

        //            _context.ChangeTracker.AutoDetectChangesEnabled = true;

        //            return new Response
        //            {
        //                IsSucced = false,
        //                Message = ex.Message
        //            };
        //        }
        //    }
    }

    /// <summary>
    /// A sheet validation failure the handler itself raises (row, field, value). Its message is written by the
    /// handler and safe to return; any other exception's text is never returned (design §9.3).
    /// </summary>
    public sealed class BulkUploadRowException(string message) : Exception(message);
}
