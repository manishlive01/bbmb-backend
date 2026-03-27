using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads;
using BBMB.BudgetService.Features.Designations.Entities;
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
using BBMB.BudgetService.Features.Works.Entities;
using BBMB.BudgetService.Infrastructure.Persistence.BudgetContexts;
using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace BBMB.BudgetService.Features.BulkUploads
{
    public class BulkUploadHandler : IRequestHandler<BulkUploadCommand, Response>
    {
        private readonly BudgetContext _context;

        public BulkUploadHandler(BudgetContext context)
        {
            _context = context;
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

            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);

                ExcelPackage.License.SetNonCommercialPersonal("BBMB");

                using var package = new ExcelPackage(stream);
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

                var type = request.uploadType.Trim().ToLower();

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
                    "workmaster" => await UploadWorkMaster(worksheet, rowCount, cancellationToken),
                    _ => new Response
                    {
                        IsSucced = false,
                        Message = "Invalid UploadType"
                    }
                };
            }
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.InnerException?.Message ?? ex.Message
                };
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
                        throw new Exception($"Invalid Major code at row {row}");

                    if (!excelDuplicateCheck.Add(code))
                        throw new Exception($"Duplicate Major code in Excel: {code}");

                    if (dbSet.Contains(code))
                        throw new Exception($"Major already exists: {code}");

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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Empty data at row {row}");
                    }

                    // 🔹 Length validation
                    if (major.Length != 4 || subMajor.Length != 2)
                    {
                        throw new Exception(
                            $"Invalid format at row {row}. Major must be 4 & SubMajor must be 2 characters.");
                    }

                    var compositeKey = (major, subMajor);
                    var excelKey = $"{major}-{subMajor}";

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(excelKey))
                    {
                        throw new Exception($"Duplicate SubMajor in Excel: {excelKey}");
                    }

                    // 🔹 FK check
                    if (!majorSet.Contains(major))
                    {
                        throw new Exception($"Major does not exist: {major}");
                    }

                    // 🔹 DB duplicate check
                    if (subMajorSet.Contains(compositeKey))
                    {
                        throw new Exception($"SubMajor already exists: {excelKey}");
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Empty Minor at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new Exception(
                            $"Invalid Minor at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new Exception($"Duplicate Minor in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new Exception($"Minor already exists: {code}");
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Empty SubMinor at row {row}");
                    }

                    // 🔹 Format validation
                    if (minor.Length != 3 || !minor.All(char.IsDigit) ||
                        subMinor.Length != 2 || !subMinor.All(char.IsDigit))
                    {
                        throw new Exception(
                            $"Invalid format at row {row}. Minor must be 3 digit & SubMinor must be 2 digit.");
                    }

                    var compositeKey = (minor, subMinor);

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(compositeKey))
                    {
                        throw new Exception($"Duplicate SubMinor in Excel: {minor}-{subMinor}");
                    }

                    // 🔹 FK check
                    if (!minorSet.Contains(minor))
                    {
                        throw new Exception($"Minor does not exist: {minor}");
                    }

                    // 🔹 DB duplicate check
                    if (subMinorSet.Contains(compositeKey))
                    {
                        throw new Exception($"SubMinor already exists: {minor}-{subMinor}");
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Empty Detail at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new Exception(
                            $"Invalid Detail code at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new Exception($"Duplicate Detail in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new Exception($"Detail already exists: {code}");
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception(
                            $"Object code or description is empty at row {row}");
                    }

                    // 🔹 Format validation
                    if (code.Length != 3 || !code.All(char.IsDigit))
                    {
                        throw new Exception(
                            $"Invalid Object code at row {row}. Must be 3 digit numeric.");
                    }

                    // 🔹 Excel duplicate check
                    if (!excelDuplicateCheck.Add(code))
                    {
                        throw new Exception(
                            $"Duplicate Object code in Excel: {code}");
                    }

                    // 🔹 DB duplicate check
                    if (existingSet.Contains(code))
                    {
                        throw new Exception(
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

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
                        throw new Exception($"Empty HeadOfAccount at row {row}");

                    if (fullCode.Length != 17 || !fullCode.All(char.IsDigit))
                        throw new Exception($"Invalid 17 digit code at row {row}");

                    if (!excelDuplicateCheck.Add(fullCode))
                        throw new Exception($"Duplicate in Excel: {fullCode}");

                    if (existingHoaSet.Contains(fullCode))
                        throw new Exception($"HeadOfAccount already exists: {fullCode}");

                    var major = fullCode.Substring(0, 4);
                    var subMajor = fullCode.Substring(4, 2);
                    var minor = fullCode.Substring(6, 3);
                    var subMinor = fullCode.Substring(9, 2);
                    var detail = fullCode.Substring(11, 3);
                    var obj = fullCode.Substring(14, 3);

                    if (!majors.Contains(major))
                        throw new Exception($"Major missing: {major}");

                    if (!subMajors.Contains((major, subMajor)))
                        throw new Exception($"SubMajor missing: {major}{subMajor}");

                    if (!minors.Contains(minor))
                        throw new Exception($"Minor missing: {minor}");

                    if (!subMinors.Contains((minor, subMinor)))
                        throw new Exception($"SubMinor missing: {minor}{subMinor}");

                    if (!details.Contains(detail))
                        throw new Exception($"Detail missing: {detail}");

                    if (!objects.Contains(obj))
                        throw new Exception($"Object missing: {obj}");

                    list.Add(new HeadOfAccount
                    {
                        MajorAccCode = major,
                        SubMajorAccCode = subMajor,
                        MinorAccCode = minor,
                        SubMinorAccCode = subMinor,
                        DetailAccCode = detail,
                        ObjectAccCode = obj,
                        HeadDesc = desc,
                        PartyEmpRequired = "N",
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
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Invalid CECode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(projectCode))
                        throw new Exception($"Required fields missing at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(ceCode))
                        throw new Exception($"Duplicate CECode in Excel: {ceCode}");

                    // 🔹 DB duplicate
                    if (existingSet.Contains(ceCode))
                        throw new Exception($"CECode already exists: {ceCode}");

                    // 🔹 FK validation
                    if (!projectSet.Contains(projectCode))
                        throw new Exception($"ProjectCode not found: {projectCode}");

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
                        throw new Exception($"Invalid CECode at row {row}");

                    // 🔹 CRCode validation
                    if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out int crCode))
                        throw new Exception($"Invalid CRCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new Exception($"Name is required at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(crCode))
                        throw new Exception($"Duplicate CRCode in Excel: {crCode}");

                    // 🔹 DB duplicate
                    if (existingCRSet.Contains(crCode))
                        throw new Exception($"CRCode already exists: {crCode}");

                    // 🔹 FK mapping (CECode → CEId)
                    if (!ceDictionary.TryGetValue(ceCode, out int ceId))
                        throw new Exception($"CECode not found: {ceCode}");

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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

                // 🔥 STEP 2: Existing CRSection Codes
                var existingCRSectionSet = new HashSet<int>(
                    await _context.Set<CRSection>()
                        .AsNoTracking()
                        .Select(x => x.CRSectionCode)
                        .ToListAsync(cancellationToken));

                var duplicateExcelCheck = new HashSet<int>();

                // 🔥 STEP 3: DB Connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                // 🔥 STEP 4: COPY
                using var writer = conn.BeginBinaryImport(@"
            COPY budget.cr_section
            (cr_id, cr_section_code, name, ""desc"", location, date, status)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: STREAM processing
                for (int row = 2; row <= rowCount; row++)
                {
                    var crSectionCodeValue = worksheet.Cells[row, 1].Value;
                    var crCodeValue = worksheet.Cells[row, 2].Value;

                    var name = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 CRSectionCode validation
                    if (crSectionCodeValue == null || !int.TryParse(crSectionCodeValue.ToString(), out int crSectionCode))
                        throw new Exception($"Invalid CRSectionCode at row {row}");

                    // 🔹 CRCode validation
                    if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out int crCode))
                        throw new Exception($"Invalid CRCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new Exception($"Name is required at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(crSectionCode))
                        throw new Exception($"Duplicate CRSectionCode in Excel: {crSectionCode}");

                    // 🔹 DB duplicate
                    if (existingCRSectionSet.Contains(crSectionCode))
                        throw new Exception($"CRSectionCode already exists: {crSectionCode}");

                    // 🔹 FK mapping (CRCode → CRId)
                    if (!crDictionary.TryGetValue(crCode, out int crId))
                        throw new Exception($"CRCode not found: {crCode}");

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
        }
        //   private async Task<Response> UploadCRManagementMaster(
        //ExcelWorksheet worksheet,
        //int rowCount,
        //CancellationToken cancellationToken)
        //   {
        //       await using var transaction =
        //           await _context.Database.BeginTransactionAsync(cancellationToken);

        //       _context.ChangeTracker.AutoDetectChangesEnabled = false;

        //       try
        //       {
        //           var list = new List<CRManagement>();
        //           var duplicateExcelCheck = new HashSet<int>();

        //           // 🔥 ONE DB HIT – Existing CR Codes
        //           var crSet = new HashSet<int>(
        //               await _context.Set<CRManagement>()
        //                   .AsNoTracking()
        //                   .Select(x => x.CRCode)
        //                   .ToListAsync(cancellationToken));

        //           // 🔥 ONE DB HIT – Existing CE Codes (FK validation)
        //           var ceSet = new HashSet<int>(
        //               await _context.Set<CEManagement>()
        //                   .AsNoTracking()
        //                   .Select(x => x.CECode)
        //                   .ToListAsync(cancellationToken));

        //           for (int row = 2; row <= rowCount; row++)
        //           {
        //               var ceCodeValue = worksheet.Cells[row, 1].Value;
        //               var crCodeValue = worksheet.Cells[row, 2].Value;

        //               var name = worksheet.Cells[row, 3].Text?.Trim();
        //               var desc = worksheet.Cells[row, 4].Text?.Trim();
        //               var location = worksheet.Cells[row, 5].Text?.Trim();

        //               // 🔹 Convert CECode
        //               if (ceCodeValue == null || !int.TryParse(ceCodeValue.ToString(), out int ceCode))
        //               {
        //                   throw new Exception($"Invalid CECode at row {row}");
        //               }

        //               // 🔹 Convert CRCode
        //               if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out int crCode))
        //               {
        //                   throw new Exception($"Invalid CRCode at row {row}");
        //               }

        //               // 🔹 Required validation
        //               if (string.IsNullOrWhiteSpace(name))
        //               {
        //                   throw new Exception($"Required fields missing at row {row}");
        //               }

        //               // 🔹 Excel duplicate check
        //               if (!duplicateExcelCheck.Add(crCode))
        //               {
        //                   throw new Exception($"Duplicate CRCode in Excel: {crCode}");
        //               }

        //               // 🔹 DB duplicate check
        //               if (crSet.Contains(crCode))
        //               {
        //                   throw new Exception($"CRCode already exists in database: {crCode}");
        //               }

        //               // 🔹 FK validation
        //               if (!ceSet.Contains(ceCode))
        //               {
        //                   throw new Exception($"CECode does not exist: {ceCode}");
        //               }

        //               list.Add(new CRManagement
        //               {
        //                   CEId = ceCode,
        //                   CRCode = crCode,
        //                   Name = name,
        //                   Description = desc,
        //                   Location = location,
        //                   Date = DateTime.UtcNow,
        //                   Status = true,
        //                   UserCode = "SYSTEM"
        //               });
        //           }

        //           // 🔥 BATCH INSERT
        //           const int batchSize = 5000;

        //           for (int i = 0; i < list.Count; i += batchSize)
        //           {
        //               var batch = list.Skip(i).Take(batchSize).ToList();

        //               await _context.Set<CRManagement>()
        //                   .AddRangeAsync(batch, cancellationToken);

        //               await _context.SaveChangesAsync(cancellationToken);

        //               _context.ChangeTracker.Clear();
        //           }

        //           await transaction.CommitAsync(cancellationToken);

        //           _context.ChangeTracker.AutoDetectChangesEnabled = true;

        //           return new Response
        //           {
        //               IsSucced = true,
        //               Message = $"{list.Count} CRManagement records uploaded successfully"
        //           };
        //       }
        //       catch (Exception ex)
        //       {
        //           await transaction.RollbackAsync(cancellationToken);

        //           _context.ChangeTracker.AutoDetectChangesEnabled = true;

        //           return new Response
        //           {
        //               IsSucced = false,
        //               Message = ex.Message
        //           };
        //       }
        //   }



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
                        throw new Exception($"Duplicate DivisionCode {divisionCode}");

                    if (divisionSet.Contains(divisionCode))
                        throw new Exception($"DivisionCode exists {divisionCode}");

                    if (!crDict.TryGetValue(crCode, out int crId))
                        throw new Exception($"CRCode not found {crCode}");

                    if (!string.IsNullOrWhiteSpace(major) && !majorSet.Contains(major))
                        throw new Exception($"MajorAccCode not found {major}");

                    if (!string.IsNullOrWhiteSpace(acc) && !accOfficeSet.Contains(acc))
                        throw new Exception($"AccOffice not found {acc}");

                    if (!string.IsNullOrWhiteSpace(unit) && !unitSet.Contains(unit))
                        throw new Exception($"UnitCode not found {unit}");

                    if (!string.IsNullOrWhiteSpace(region) && !regionSet.Contains(region))
                        throw new Exception($"RegionCode not found {region}");

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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

                // 🔥 STEP 2: Existing DivSection Codes
                var existingSet = new HashSet<int>(
                    await _context.Set<DivSection>()
                        .AsNoTracking()
                        .Select(x => x.DivSectionCode)
                        .ToListAsync(cancellationToken));

                var duplicateExcelCheck = new HashSet<int>();

                // 🔥 STEP 3: DB Connection
                var conn = (Npgsql.NpgsqlConnection)_context.Database.GetDbConnection();
                await conn.OpenAsync(cancellationToken);

                // 🔥 STEP 4: COPY
                using var writer = conn.BeginBinaryImport(@"
            COPY budget.div_section
            (division_id, div_section_code, name, ""desc"", location, date, status)
            FROM STDIN (FORMAT BINARY)");

                int successCount = 0;

                // 🔥 STEP 5: STREAM processing
                for (int row = 2; row <= rowCount; row++)
                {
                    var divSectionCodeValue = worksheet.Cells[row, 1].Value;
                    var divisionCodeValue = worksheet.Cells[row, 2].Value;

                    var name = worksheet.Cells[row, 3].Text?.Trim();
                    var desc = worksheet.Cells[row, 4].Text?.Trim();
                    var location = worksheet.Cells[row, 5].Text?.Trim();

                    // 🔹 DivSectionCode validation
                    if (divSectionCodeValue == null || !int.TryParse(divSectionCodeValue.ToString(), out int divSectionCode))
                        throw new Exception($"Invalid DivSectionCode at row {row}");

                    // 🔹 DivisionCode validation
                    if (divisionCodeValue == null || !int.TryParse(divisionCodeValue.ToString(), out int divisionCode))
                        throw new Exception($"Invalid DivisionCode at row {row}");

                    // 🔹 Required validation
                    if (string.IsNullOrWhiteSpace(name))
                        throw new Exception($"Name is required at row {row}");

                    // 🔹 Excel duplicate
                    if (!duplicateExcelCheck.Add(divSectionCode))
                        throw new Exception($"Duplicate DivSectionCode in Excel: {divSectionCode}");

                    // 🔹 DB duplicate
                    if (existingSet.Contains(divSectionCode))
                        throw new Exception($"DivSectionCode already exists: {divSectionCode}");

                    // 🔹 FK mapping (DivisionCode → DivisionId)
                    if (!divisionDict.TryGetValue(divisionCode, out int divisionId))
                        throw new Exception($"DivisionCode not found: {divisionCode}");

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
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
        //                    throw new Exception($"Required fields missing at row {row}");
        //                }

        //                if (!int.TryParse(divisionCodeText, out var divisionCode))
        //                    throw new Exception($"Invalid DivisionCode '{divisionCodeText}' at row {row}");

        //                if (!int.TryParse(divSectionText, out var divSection))
        //                    throw new Exception($"Invalid DivSection '{divSectionText}' at row {row}");

        //                if (ceCodeValue == null || !int.TryParse(ceCodeValue.ToString(), out var ceCode))
        //                    throw new Exception($"Invalid CECode at row {row}");

        //                if (crCodeValue == null || !int.TryParse(crCodeValue.ToString(), out var crCode))
        //                    throw new Exception($"Invalid CRCode at row {row}");

        //                int? parentDivision = null;
        //                if (!string.IsNullOrWhiteSpace(parentDivisionText))
        //                {
        //                    if (!int.TryParse(parentDivisionText, out var parent))
        //                        throw new Exception($"Invalid ParentDivisionCode '{parentDivisionText}' at row {row}");

        //                    parentDivision = parent;
        //                }

        //                // 🔹 Excel duplicate
        //                if (!excelDuplicateCheck.Add(divisionCode))
        //                    throw new Exception($"Duplicate DivisionCode '{divisionCode}' in Excel (row {row})");

        //                // 🔹 DB duplicate
        //                if (divisionSet.Contains(divisionCode))
        //                    throw new Exception($"DivisionCode '{divisionCode}' already exists in database (row {row})");

        //                // 🔹 FK validations
        //                if (!ceSet.Contains(ceCode))
        //                    throw new Exception($"CECode '{ceCode}' not found (row {row})");

        //                if (!crSet.Contains(crCode))
        //                    throw new Exception($"CRCode '{crCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(majorAccCode) && !majorSet.Contains(majorAccCode))
        //                    throw new Exception($"MajorAccCode '{majorAccCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(roCode) && !roSet.Contains(roCode))
        //                    throw new Exception($"RoCode '{roCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(accOffice) && !accOfficeSet.Contains(accOffice))
        //                    throw new Exception($"AccOffice '{accOffice}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(unitCode) && !unitSet.Contains(unitCode))
        //                    throw new Exception($"UnitCode '{unitCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(poolCode) && !poolSet.Contains(poolCode))
        //                    throw new Exception($"PoolCode '{poolCode}' not found (row {row})");

        //                if (!string.IsNullOrWhiteSpace(regionCode) && !regionSet.Contains(regionCode))
        //                    throw new Exception($"RegionCode '{regionCode}' not found (row {row})");

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

        // ============================================================
        // WORK MASTER
        // ============================================================
        private async Task<Response> UploadWorkMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync(cancellationToken);

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                var list = new List<Work>();
                var duplicateExcelCheck = new HashSet<string>();

                // 🔥 PRELOAD DATA (ONE TIME DB HITS)

                var workSet = new HashSet<int>(
                    await _context.Set<Work>()
                        .AsNoTracking()
                        .Select(x => x.WorkCode)
                        .ToListAsync(cancellationToken));

                var divisionSet = new HashSet<int>(
                    await _context.Set<Division>()
                        .AsNoTracking()
                        .Select(x => x.DivisionCode)
                        .ToListAsync(cancellationToken));

                var majorSet = new HashSet<string>(
                    await _context.Set<MajorHead>()
                        .AsNoTracking()
                        .Select(x => x.MajorAccCode)
                        .ToListAsync(cancellationToken));

                var subMajorSet = new HashSet<(string, string)>(
                    (await _context.Set<SubMajorHead>()
                        .AsNoTracking()
                        .Select(x => new { x.MajorAccCode, x.SubMajorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MajorAccCode, x.SubMajorAccCode)));

                var minorSet = new HashSet<string>(
                    await _context.Set<MinorHead>()
                        .AsNoTracking()
                        .Select(x => x.MinorAccCode)
                        .ToListAsync(cancellationToken));

                var subMinorSet = new HashSet<(string, string)>(
                    (await _context.Set<SubMinorHead>()
                        .AsNoTracking()
                        .Select(x => new { x.MinorAccCode, x.SubMinorAccCode })
                        .ToListAsync(cancellationToken))
                    .Select(x => (x.MinorAccCode, x.SubMinorAccCode)));

                var detailSet = new HashSet<string>(
                    await _context.Set<DetailHead>()
                        .AsNoTracking()
                        .Select(x => x.DetailAccCode)
                        .ToListAsync(cancellationToken));

                var objectSet = new HashSet<string>(
                    await _context.Set<ObjectClassification>()
                        .AsNoTracking()
                        .Select(x => x.ObjectAccCode)
                        .ToListAsync(cancellationToken));

                var designationSet = new HashSet<string>(
                    await _context.Set<Designation>()
                        .AsNoTracking()
                        .Select(x => x.DesignationCode)
                        .ToListAsync(cancellationToken));

                for (int row = 2; row <= rowCount; row++)
                {
                    var workCode = worksheet.Cells[row, 1].Text?.Trim();
                    var divisionText = worksheet.Cells[row, 2].Text?.Trim();
                    var divSectionText = worksheet.Cells[row, 3].Text?.Trim();
                    var majorCode = worksheet.Cells[row, 4].Text?.Trim();
                    var subMajorCode = worksheet.Cells[row, 5].Text?.Trim();
                    var minorCode = worksheet.Cells[row, 6].Text?.Trim();
                    var subMinorCode = worksheet.Cells[row, 7].Text?.Trim();
                    var detailCode = worksheet.Cells[row, 8].Text?.Trim();
                    var objectCode = worksheet.Cells[row, 9].Text?.Trim();
                    var finYrStart = worksheet.Cells[row, 10].Text?.Trim();
                    var finYrEnd = worksheet.Cells[row, 11].Text?.Trim();
                    var sanctionNo = worksheet.Cells[row, 12].Text?.Trim();
                    var sanctionDate = worksheet.Cells[row, 13].Text?.Trim();
                    var designationCode = worksheet.Cells[row, 14].Text?.Trim();
                    var estimatedCostText = worksheet.Cells[row, 15].Text?.Trim();
                    var status = worksheet.Cells[row, 16].Text?.Trim();
                    var workDesc = worksheet.Cells[row, 17].Text?.Trim();
                    var nature = worksheet.Cells[row, 18].Text?.Trim();
                    var refWorkCode = worksheet.Cells[row, 19].Text?.Trim();

                    // Required validation
                    if (string.IsNullOrWhiteSpace(workCode) ||
                        string.IsNullOrWhiteSpace(divisionText) ||
                        string.IsNullOrWhiteSpace(divSectionText) ||
                        string.IsNullOrWhiteSpace(majorCode) ||
                        string.IsNullOrWhiteSpace(subMajorCode) ||
                        string.IsNullOrWhiteSpace(minorCode) ||
                        string.IsNullOrWhiteSpace(subMinorCode) ||
                        string.IsNullOrWhiteSpace(detailCode) ||
                        string.IsNullOrWhiteSpace(objectCode) ||
                        string.IsNullOrWhiteSpace(workDesc))
                        throw new Exception($"Required fields missing at row {row}");

                    if (!int.TryParse(divisionText, out int divisionCode))
                        throw new Exception($"Invalid DivisionCode at row {row}");

                    if (!int.TryParse(divSectionText, out int divSection))
                        throw new Exception($"Invalid DivSection at row {row}");

                    decimal? estimatedCost = null;
                    if (!string.IsNullOrWhiteSpace(estimatedCostText))
                    {
                        if (!decimal.TryParse(estimatedCostText, out decimal cost))
                            throw new Exception($"Invalid EstimatedCost at row {row}");

                        estimatedCost = cost;
                    }

                    if (!duplicateExcelCheck.Add(workCode))
                        throw new Exception($"Duplicate WorkCode in Excel: {workCode}");

                    if (workSet.Contains(Convert.ToInt32(workCode)))
                        throw new Exception($"WorkCode already exists: {workCode}");

                    // FK validations
                    if (!divisionSet.Contains(divisionCode))
                        throw new Exception($"Invalid DivisionCode at row {row}");

                    if (!majorSet.Contains(majorCode))
                        throw new Exception($"Invalid MajorAccCode at row {row}");

                    if (!subMajorSet.Contains((majorCode, subMajorCode)))
                        throw new Exception($"Invalid SubMajorAccCode at row {row}");

                    if (!minorSet.Contains(minorCode))
                        throw new Exception($"Invalid MinorAccCode at row {row}");

                    if (!subMinorSet.Contains((minorCode, subMinorCode)))
                        throw new Exception($"Invalid SubMinorAccCode at row {row}");

                    if (!detailSet.Contains(detailCode))
                        throw new Exception($"Invalid DetailAccCode at row {row}");

                    if (!objectSet.Contains(objectCode))
                        throw new Exception($"Invalid ObjectAccCode at row {row}");

                    if (!string.IsNullOrWhiteSpace(designationCode) &&
                        !designationSet.Contains(designationCode))
                        throw new Exception($"Invalid DesignationCode at row {row}");

                    if (!string.IsNullOrWhiteSpace(refWorkCode) &&
                        !workSet.Contains(Convert.ToInt32(refWorkCode)))
                        throw new Exception($"Invalid RefWorkCode at row {row}");

                    list.Add(new Work
                    {
                        //WorkCode = workCode,
                        //DivisionCode = divisionCode,
                        //DivSection = divSection,
                        MajorAccCode = majorCode,
                        SubMajorAccCode = subMajorCode,
                        MinorAccCode = minorCode,
                        SubMinorAccCode = subMinorCode,
                        DetailAccCode = detailCode,
                        ObjectAccCode = objectCode,
                        FinYrStart = finYrStart,
                        FinYrEnd = finYrEnd,
                        SanctionNo = sanctionNo,
                        SanctionDate = sanctionDate,
                        DesignationCode = designationCode,
                        EstimatedCost = estimatedCost,
                        Status = string.IsNullOrWhiteSpace(status) ? "P" : status,
                        WorkDesc = workDesc,
                        Nature = nature,
                        RefWorkCode = refWorkCode,
                        UserCode = "SYSTEM"
                    });
                }

                // 🔥 BATCH INSERT
                const int batchSize = 5000;

                for (int i = 0; i < list.Count; i += batchSize)
                {
                    var batch = list.Skip(i).Take(batchSize).ToList();

                    await _context.Set<Work>().AddRangeAsync(batch, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _context.ChangeTracker.Clear();
                }

                await transaction.CommitAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = true,
                    Message = $"{list.Count} Work records uploaded successfully"
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                _context.ChangeTracker.AutoDetectChangesEnabled = true;

                return new Response
                {
                    IsSucced = false,
                    Message = ex.Message
                };
            }
        }

       
    }
}