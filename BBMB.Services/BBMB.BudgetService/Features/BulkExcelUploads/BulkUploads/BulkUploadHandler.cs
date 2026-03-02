using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads;
using BBMB.BudgetService.Features.CEManagements.Entities;
using BBMB.BudgetService.Features.CRManagements.Entities;
using BBMB.BudgetService.Features.Designations.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
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
                return new Response { IsSucced = false, Message = "File is required" };

            if (string.IsNullOrWhiteSpace(request.uploadType))
                return new Response { IsSucced = false, Message = "UploadType is required" };

            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);

                ExcelPackage.License.SetNonCommercialPersonal("BBMB");

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return new Response { IsSucced = false, Message = "Invalid Excel file" };

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                    return new Response { IsSucced = false, Message = "Excel has no data" };

                var type = request.uploadType.Trim().ToLower();

                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                Response result = type switch
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
                    "divisionmaster" => await UploadDivisionMaster(worksheet, rowCount, cancellationToken),
                    "workmaster" => await UploadWorkMaster(worksheet, rowCount, cancellationToken),
                    _ => new Response { IsSucced = false, Message = "Invalid UploadType" }
                };

                if (!result.IsSucced)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                await transaction.CommitAsync(cancellationToken);
                return result;
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
        //    private async Task<Response> UploadMajorMaster(
        //ExcelWorksheet worksheet,
        //int rowCount,
        //CancellationToken cancellationToken)
        //    {
        //        var list = new List<MajorHead>();
        //        var duplicateCheck = new HashSet<string>();

        //        for (int row = 2; row <= rowCount; row++)
        //        {
        //            var code = worksheet.Cells[row, 1].Text.Trim();
        //            var desc = worksheet.Cells[row, 2].Text.Trim();

        //            if (code.Length != 4)
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Invalid Major code at row {row}"
        //                };

        //            if (!duplicateCheck.Add(code))
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Duplicate Major code in Excel: {code}"
        //                };

        //            if (await _context.MajorHeads.AsNoTracking()
        //                .AnyAsync(x => x.MajorAccCode == code, cancellationToken))
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Major already exists: {code}"
        //                };

        //            list.Add(new MajorHead
        //            {
        //                MajorAccCode = code,
        //                MajorAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });
        //        }

        //        await _context.MajorHeads.AddRangeAsync(list, cancellationToken);
        //        await _context.SaveChangesAsync(cancellationToken);

        //        return new Response
        //        {
        //            IsSucced = true,
        //            Message = "Major master uploaded successfully"
        //        };
        //    }

        private async Task<Response> UploadMajorMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<MajorHead>();
            var excelDuplicateCheck = new HashSet<string>();

            // 🔥 ONE DB HIT ONLY
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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Major code at row {row}"
                    };

                // Excel duplicate check
                if (!excelDuplicateCheck.Add(code))
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Major code in Excel: {code}"
                    };

                // DB duplicate check (memory)
                if (dbSet.Contains(code))
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major already exists: {code}"
                    };

                list.Add(new MajorHead
                {
                    MajorAccCode = code,
                    MajorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Safe for Lakhs)
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

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Major records uploaded successfully"
            };
        }

        // ============================================================
        // SUB MAJOR HEAD
        // ============================================================
        private async Task<Response> UploadSubMajorMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Empty data at row {row}"
                    };
                }

                // 🔹 Length validation
                if (major.Length != 4 || subMajor.Length != 2)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid format at row {row}. Major must be 4 & SubMajor must be 2 characters."
                    };
                }

                var compositeKey = (major, subMajor);
                var excelKey = $"{major}-{subMajor}";

                // 🔹 Excel duplicate check
                if (!excelDuplicateCheck.Add(excelKey))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate SubMajor in Excel: {excelKey}"
                    };
                }

                // 🔹 FK check
                if (!majorSet.Contains(major))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major does not exist: {major}"
                    };
                }

                // 🔹 DB duplicate check
                if (subMajorSet.Contains(compositeKey))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"SubMajor already exists: {excelKey}"
                    };
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

            // 🔥 BATCH INSERT (Scalable)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.SubMajorHeads.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} SubMajor records uploaded successfully"
            };
        }
        // ============================================================
        // MINOR HEAD
        // ============================================================
        private async Task<Response> UploadMinorMaster(
     ExcelWorksheet worksheet,
     int rowCount,
     CancellationToken cancellationToken)
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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Empty Minor at row {row}"
                    };
                }

                // 🔹 Format validation
                if (code.Length != 3 || !code.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Minor at row {row}. Must be 3 digit numeric."
                    };
                }

                // 🔹 Excel duplicate check
                if (!excelDuplicateCheck.Add(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Minor in Excel: {code}"
                    };
                }

                // 🔹 DB duplicate check
                if (existingSet.Contains(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor already exists: {code}"
                    };
                }

                list.Add(new MinorHead
                {
                    MinorAccCode = code,
                    MinorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Scalable)
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

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Minor records uploaded successfully"
            };
        }
        // ============================================================
        // SUB MINOR HEAD
        // ============================================================
        private async Task<Response> UploadSubMinorMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<SubMinorHead>();
            var excelDuplicateCheck = new HashSet<string>();

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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Empty SubMinor at row {row}"
                    };
                }

                // 🔹 Format validation
                if (minor.Length != 3 || !minor.All(char.IsDigit) ||
                    subMinor.Length != 2 || !subMinor.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid format at row {row}. Minor must be 3 digit & SubMinor must be 2 digit."
                    };
                }

                var compositeKey = (minor, subMinor);
                var excelKey = $"{minor}-{subMinor}";

                // 🔹 Excel duplicate check
                if (!excelDuplicateCheck.Add(excelKey))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate SubMinor in Excel: {excelKey}"
                    };
                }

                // 🔹 FK check
                if (!minorSet.Contains(minor))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor does not exist: {minor}"
                    };
                }

                // 🔹 DB duplicate check
                if (subMinorSet.Contains(compositeKey))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"SubMinor already exists: {excelKey}"
                    };
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

            // 🔥 BATCH INSERT (Scalable)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.SubMinorHeads.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} SubMinor records uploaded successfully"
            };
        }

        // ============================================================
        // DETAIL HEAD
        // ============================================================
        private async Task<Response> UploadDetailMaster(
      ExcelWorksheet worksheet,
      int rowCount,
      CancellationToken cancellationToken)
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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Empty Detail at row {row}"
                    };
                }

                // 🔹 Format validation
                if (code.Length != 3 || !code.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Detail code at row {row}. Must be 3 digit numeric."
                    };
                }

                // 🔹 Excel duplicate check
                if (!excelDuplicateCheck.Add(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Detail in Excel: {code}"
                    };
                }

                // 🔹 DB duplicate check
                if (existingSet.Contains(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Detail already exists: {code}"
                    };
                }

                list.Add(new DetailHead
                {
                    DetailAccCode = code,
                    DetailAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Scalable for lakhs)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.DetailHeads.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Detail records uploaded successfully"
            };
        }


        // ============================================================
        // OBJECT CLASSIFICATION
        // ============================================================
        private async Task<Response> UploadObjectMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
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
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Object code or description is empty at row {row}"
                    };
                }

                // 🔹 Format validation
                if (code.Length != 3 || !code.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Object code at row {row}. Must be 3 digit numeric."
                    };
                }

                // 🔹 Excel duplicate check
                if (!excelDuplicateCheck.Add(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Object code in Excel: {code}"
                    };
                }

                // 🔹 DB duplicate check
                if (existingSet.Contains(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Object already exists in database: {code}"
                    };
                }

                list.Add(new ObjectClassification
                {
                    ObjectAccCode = code,
                    ObjectAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Scalable)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.ObjectClassifications.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Object records uploaded successfully"
            };
        }

        // ============================================================
        // HEAD OF ACCOUNT MASTER
        // ============================================================
        private async Task<Response> UploadHeadOfAccountMaster(
     ExcelWorksheet worksheet,
     int rowCount,
     CancellationToken cancellationToken)
        {
            var list = new List<HeadOfAccount>();
            var excelDuplicateCheck = new HashSet<string>();

            // 🔥 Preload all required master data (single DB hits)

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
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Empty HeadOfAccount at row {row}"
                    };
                }

                if (fullCode.Length != 17 || !fullCode.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid 17 digit code at row {row}"
                    };
                }

                if (!excelDuplicateCheck.Add(fullCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate in Excel: {fullCode}"
                    };
                }

                if (existingHoaSet.Contains(fullCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"HeadOfAccount already exists: {fullCode}"
                    };
                }

                var major = fullCode.Substring(0, 4);
                var subMajor = fullCode.Substring(4, 2);
                var minor = fullCode.Substring(6, 3);
                var subMinor = fullCode.Substring(9, 2);
                var detail = fullCode.Substring(11, 3);
                var obj = fullCode.Substring(14, 3);

                if (!majors.Contains(major))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Major missing: {major}"
                    };
                }

                if (!subMajors.Contains((major, subMajor)))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"SubMajor missing: {major}{subMajor}"
                    };
                }

                if (!minors.Contains(minor))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor missing: {minor}"
                    };
                }

                if (!subMinors.Contains((minor, subMinor)))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"SubMinor missing: {minor}{subMinor}"
                    };
                }

                if (!details.Contains(detail))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Detail missing: {detail}"
                    };
                }

                if (!objects.Contains(obj))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Object missing: {obj}"
                    };
                }

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

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.HeadOfAccounts.AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} HeadOfAccount records uploaded successfully"
            };
        }

        // ============================================================
        // CE MANAGEMENT MASTER
        // ============================================================
        private async Task<Response> UploadCEManagementMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<CEManagement>();
            var duplicateExcelCheck = new HashSet<string>();

            // 🔥 ONE DB HIT – Existing CE Codes
            var existingSet = new HashSet<string>(
                await _context.Set<CEManagement>()
                    .AsNoTracking()
                    .Select(x => x.CECode)
                    .ToListAsync(cancellationToken));

            // 🔥 ONE DB HIT – Existing Project Codes (FK validation)
            var projectSet = new HashSet<string>(
                await _context.Set<Project>()
                    .AsNoTracking()
                    .Select(x => x.ProjCode)
                    .ToListAsync(cancellationToken));

            for (int row = 2; row <= rowCount; row++)
            {
                var ceCode = worksheet.Cells[row, 1].Text?.Trim();
                var name = worksheet.Cells[row, 2].Text?.Trim();
                var projectCode = worksheet.Cells[row, 3].Text?.Trim();
                var desc = worksheet.Cells[row, 4].Text?.Trim();
                var location = worksheet.Cells[row, 5].Text?.Trim();

                // 🔹 Required field validation
                if (string.IsNullOrWhiteSpace(ceCode) ||
                    string.IsNullOrWhiteSpace(name) ||
                    string.IsNullOrWhiteSpace(projectCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Required fields missing at row {row}"
                    };
                }

                // 🔹 Excel duplicate check
                if (!duplicateExcelCheck.Add(ceCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate CECode in Excel: {ceCode}"
                    };
                }

                // 🔹 DB duplicate check
                if (existingSet.Contains(ceCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"CECode already exists in database: {ceCode}"
                    };
                }

                // 🔹 FK check
                if (!projectSet.Contains(projectCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"ProjectCode does not exist: {projectCode}"
                    };
                }

                list.Add(new CEManagement
                {
                    CECode = ceCode,
                    Name = name,
                    ProjectCode = projectCode,
                    Description = desc,
                    Location = location,
                    Date = DateTime.UtcNow,
                    Status = true,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Scalable for lakhs)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.Set<CEManagement>()
                    .AddRangeAsync(batch, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} CEManagement records uploaded successfully"
            };
        }


        // ============================================================
        // CR MANAGEMENT MASTER
        // ============================================================
        private async Task<Response> UploadCRManagementMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<CRManagement>();
            var duplicateExcelCheck = new HashSet<string>();

            // 🔥 ONE DB HIT – Existing CR codes
            var crSet = new HashSet<string>(
                await _context.Set<CRManagement>()
                    .AsNoTracking()
                    .Select(x => x.CRCode)
                    .ToListAsync(cancellationToken));

            // 🔥 ONE DB HIT – Existing CE codes (FK validation)
            var ceSet = new HashSet<string>(
                await _context.Set<CEManagement>()
                    .AsNoTracking()
                    .Select(x => x.CECode)
                    .ToListAsync(cancellationToken));

            for (int row = 2; row <= rowCount; row++)
            {
                var ceCode = worksheet.Cells[row, 1].Text?.Trim();
                var crCode = worksheet.Cells[row, 2].Text?.Trim();
                var name = worksheet.Cells[row, 3].Text?.Trim();
                var desc = worksheet.Cells[row, 4].Text?.Trim();
                var location = worksheet.Cells[row, 5].Text?.Trim();

                // 🔹 Required field validation
                if (string.IsNullOrWhiteSpace(ceCode) ||
                    string.IsNullOrWhiteSpace(crCode) ||
                    string.IsNullOrWhiteSpace(name))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Required fields missing at row {row}"
                    };
                }

                // 🔹 Excel duplicate check
                if (!duplicateExcelCheck.Add(crCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate CRCode in Excel: {crCode}"
                    };
                }

                // 🔹 DB duplicate check
                if (crSet.Contains(crCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"CRCode already exists in database: {crCode}"
                    };
                }

                // 🔹 FK validation
                if (!ceSet.Contains(ceCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"CECode does not exist: {ceCode}"
                    };
                }

                list.Add(new CRManagement
                {
                    CECode = ceCode,
                    CRCode = crCode,
                    Name = name,
                    Description = desc,
                    Location = location,
                    Date = DateTime.UtcNow,
                    Status = true,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 BATCH INSERT (Scalable for lakhs)
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.Set<CRManagement>()
                    .AddRangeAsync(batch, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} CRManagement records uploaded successfully"
            };
        }

        // ============================================================
        // DIVISION MASTER
        // ============================================================
        private async Task<Response> UploadDivisionMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<Division>();
            var excelDuplicateCheck = new HashSet<int>();

            // 🔥 ONE TIME DB LOADS

            var divisionSet = new HashSet<int>(
                await _context.Set<Division>()
                    .AsNoTracking()
                    .Select(x => x.DivisionCode)
                    .ToListAsync(cancellationToken));

            var ceSet = new HashSet<string>(
                await _context.Set<CEManagement>()
                    .AsNoTracking()
                    .Select(x => x.CECode)
                    .ToListAsync(cancellationToken));

            var crSet = new HashSet<string>(
                await _context.Set<CRManagement>()
                    .AsNoTracking()
                    .Select(x => x.CRCode)
                    .ToListAsync(cancellationToken));

            var majorSet = new HashSet<string>(
                await _context.Set<MajorHead>()
                    .AsNoTracking()
                    .Select(x => x.MajorAccCode)
                    .ToListAsync(cancellationToken));

            var roSet = new HashSet<string>(
                await _context.Set<RemittanceOffice>()
                    .AsNoTracking()
                    .Select(x => x.RoCode)
                    .ToListAsync(cancellationToken));

            var accOfficeSet = new HashSet<string>(
                await _context.Set<AccountOffice>()
                    .AsNoTracking()
                    .Select(x => x.AccOffice)
                    .ToListAsync(cancellationToken));

            var unitSet = new HashSet<string>(
                await _context.Set<UnitMaster>()
                    .AsNoTracking()
                    .Select(x => x.UnitCode)
                    .ToListAsync(cancellationToken));

            var poolSet = new HashSet<string>(
                await _context.Set<Pool>()
                    .AsNoTracking()
                    .Select(x => x.PoolCode)
                    .ToListAsync(cancellationToken));

            var regionSet = new HashSet<string>(
                await _context.Set<Region>()
                    .AsNoTracking()
                    .Select(x => x.RegionCode)
                    .ToListAsync(cancellationToken));

            for (int row = 2; row <= rowCount; row++)
            {
                var divisionCodeText = worksheet.Cells[row, 1].Text?.Trim();
                var divSectionText = worksheet.Cells[row, 2].Text?.Trim();
                var divisionDesc = worksheet.Cells[row, 3].Text?.Trim();
                var ceCode = worksheet.Cells[row, 4].Text?.Trim();
                var crCode = worksheet.Cells[row, 5].Text?.Trim();
                var majorAccCode = worksheet.Cells[row, 6].Text?.Trim();
                var roCode = worksheet.Cells[row, 7].Text?.Trim();
                var accOffice = worksheet.Cells[row, 8].Text?.Trim();
                var unitCode = worksheet.Cells[row, 9].Text?.Trim();
                var poolCode = worksheet.Cells[row, 10].Text?.Trim();
                var regionCode = worksheet.Cells[row, 11].Text?.Trim();
                var parentDivisionText = worksheet.Cells[row, 12].Text?.Trim();

                // Required fields
                if (string.IsNullOrWhiteSpace(divisionCodeText) ||
                    string.IsNullOrWhiteSpace(divSectionText) ||
                    string.IsNullOrWhiteSpace(divisionDesc) ||
                    string.IsNullOrWhiteSpace(ceCode) ||
                    string.IsNullOrWhiteSpace(crCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Required fields missing at row {row}"
                    };
                }

                if (!int.TryParse(divisionCodeText, out var divisionCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid DivisionCode '{divisionCodeText}' at row {row}"
                    };
                }

                if (!int.TryParse(divSectionText, out var divSection))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid DivSection '{divSectionText}' at row {row}"
                    };
                }

                int? parentDivision = null;
                if (!string.IsNullOrWhiteSpace(parentDivisionText))
                {
                    if (!int.TryParse(parentDivisionText, out var parent))
                    {
                        return new Response
                        {
                            IsSucced = false,
                            Message = $"Invalid ParentDivisionCode '{parentDivisionText}' at row {row}"
                        };
                    }
                    parentDivision = parent;
                }

                // Excel duplicate
                if (!excelDuplicateCheck.Add(divisionCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate DivisionCode '{divisionCode}' in Excel (row {row})"
                    };
                }

                // DB duplicate
                if (divisionSet.Contains(divisionCode))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"DivisionCode '{divisionCode}' already exists in database (row {row})"
                    };
                }

                // FK Validations
                if (!ceSet.Contains(ceCode))
                    return new Response { IsSucced = false, Message = $"CECode '{ceCode}' not found (row {row})" };

                if (!crSet.Contains(crCode))
                    return new Response { IsSucced = false, Message = $"CRCode '{crCode}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(majorAccCode) && !majorSet.Contains(majorAccCode))
                    return new Response { IsSucced = false, Message = $"MajorAccCode '{majorAccCode}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(roCode) && !roSet.Contains(roCode))
                    return new Response { IsSucced = false, Message = $"RoCode '{roCode}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(accOffice) && !accOfficeSet.Contains(accOffice))
                    return new Response { IsSucced = false, Message = $"AccOffice '{accOffice}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(unitCode) && !unitSet.Contains(unitCode))
                    return new Response { IsSucced = false, Message = $"UnitCode '{unitCode}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(poolCode) && !poolSet.Contains(poolCode))
                    return new Response { IsSucced = false, Message = $"PoolCode '{poolCode}' not found (row {row})" };

                if (!string.IsNullOrWhiteSpace(regionCode) && !regionSet.Contains(regionCode))
                    return new Response { IsSucced = false, Message = $"RegionCode '{regionCode}' not found (row {row})" };

                list.Add(new Division
                {
                    DivisionCode = divisionCode,
                    DivSection = divSection,
                    DivisionDesc = divisionDesc,
                    CECode = ceCode,
                    CRCode = crCode,
                    MajorAccCode = majorAccCode,
                    RoCode = roCode,
                    AccOffice = accOffice,
                    UnitCode = unitCode,
                    PoolCode = poolCode,
                    RegionCode = regionCode,
                    ParentDivisionCode = parentDivision,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // 🔥 Batch Insert
            const int batchSize = 5000;

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.Set<Division>().AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Division records uploaded successfully"
            };
        }

        // ============================================================
        // WORK MASTER
        // ============================================================
        private async Task<Response> UploadWorkMaster(
    ExcelWorksheet worksheet,
    int rowCount,
    CancellationToken cancellationToken)
        {
            var list = new List<Work>();
            var duplicateExcelCheck = new HashSet<string>();

            // 🔥 PRELOAD ALL REQUIRED DATA (ONE TIME DB HITS)

            var workSet = new HashSet<string>(
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

            var subMajorSet = new HashSet<(string Major, string SubMajor)>(
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

            var subMinorSet = new HashSet<(string Minor, string SubMinor)>(
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
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Required fields missing at row {row}"
                    };
                }

                if (!int.TryParse(divisionText, out int divisionCode))
                    return new Response { IsSucced = false, Message = $"Invalid DivisionCode at row {row}" };

                if (!int.TryParse(divSectionText, out int divSection))
                    return new Response { IsSucced = false, Message = $"Invalid DivSection at row {row}" };

                decimal? estimatedCost = null;
                if (!string.IsNullOrWhiteSpace(estimatedCostText))
                {
                    if (!decimal.TryParse(estimatedCostText, out decimal cost))
                        return new Response { IsSucced = false, Message = $"Invalid EstimatedCost at row {row}" };

                    estimatedCost = cost;
                }

                if (!duplicateExcelCheck.Add(workCode))
                    return new Response { IsSucced = false, Message = $"Duplicate WorkCode in Excel: {workCode}" };

                if (workSet.Contains(workCode))
                    return new Response { IsSucced = false, Message = $"WorkCode already exists: {workCode}" };

                // FK validation
                if (!divisionSet.Contains(divisionCode))
                    return new Response { IsSucced = false, Message = $"Invalid DivisionCode at row {row}" };

                if (!majorSet.Contains(majorCode))
                    return new Response { IsSucced = false, Message = $"Invalid MajorAccCode at row {row}" };

                if (!subMajorSet.Contains((majorCode, subMajorCode)))
                    return new Response { IsSucced = false, Message = $"Invalid SubMajorAccCode at row {row}" };

                if (!minorSet.Contains(minorCode))
                    return new Response { IsSucced = false, Message = $"Invalid MinorAccCode at row {row}" };

                if (!subMinorSet.Contains((minorCode, subMinorCode)))
                    return new Response { IsSucced = false, Message = $"Invalid SubMinorAccCode at row {row}" };

                if (!detailSet.Contains(detailCode))
                    return new Response { IsSucced = false, Message = $"Invalid DetailAccCode at row {row}" };

                if (!objectSet.Contains(objectCode))
                    return new Response { IsSucced = false, Message = $"Invalid ObjectAccCode at row {row}" };

                if (!string.IsNullOrWhiteSpace(designationCode) &&
                    !designationSet.Contains(designationCode))
                    return new Response { IsSucced = false, Message = $"Invalid DesignationCode at row {row}" };

                if (!string.IsNullOrWhiteSpace(refWorkCode) &&
                    !workSet.Contains(refWorkCode))
                    return new Response { IsSucced = false, Message = $"Invalid RefWorkCode at row {row}" };

                list.Add(new Work
                {
                    WorkCode = workCode,
                    DivisionCode = divisionCode,
                    DivSection = divSection,
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

            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            for (int i = 0; i < list.Count; i += batchSize)
            {
                var batch = list.Skip(i).Take(batchSize).ToList();

                await _context.Set<Work>().AddRangeAsync(batch, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _context.ChangeTracker.Clear();
            }

            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} Work records uploaded successfully"
            };
        }

        //    // ============================================================
        //    // HEAD OF ACCOUNT - FULL SAFE INSERT
        //    // ============================================================
        //    private async Task<Response> UploadHeadOfAccounts(
        //ExcelWorksheet worksheet,
        //int rowCount,
        //CancellationToken cancellationToken)
        //    {
        //        var majorList = new List<MajorHead>();
        //        var subMajorList = new List<SubMajorHead>();
        //        var minorList = new List<MinorHead>();
        //        var subMinorList = new List<SubMinorHead>();
        //        var detailList = new List<DetailHead>();
        //        var objectList = new List<ObjectClassification>();
        //        var headMasterList = new List<HeadOfAccount>();

        //        var excelDuplicateCheck = new HashSet<string>();

        //        for (int row = 2; row <= rowCount; row++)
        //        {
        //            var fullCode = worksheet.Cells[row, 1].Text?.Trim();
        //            var desc = worksheet.Cells[row, 2].Text?.Trim();

        //            // ================= VALIDATION =================

        //            if (string.IsNullOrWhiteSpace(fullCode) ||
        //                string.IsNullOrWhiteSpace(desc))
        //            {
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Invalid or empty data at row {row}"
        //                };
        //            }

        //            if (fullCode.Length != 17 || !fullCode.All(char.IsDigit))
        //            {
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Invalid 17-digit Head Of Account at row {row}"
        //                };
        //            }

        //            // Duplicate inside Excel check
        //            if (!excelDuplicateCheck.Add(fullCode))
        //            {
        //                return new Response
        //                {
        //                    IsSucced = false,
        //                    Message = $"Duplicate Head Of Account in Excel: {fullCode}"
        //                };
        //            }

        //            // ================= SPLIT 17 DIGIT =================

        //            var major = fullCode.Substring(0, 4);
        //            var subMajor = fullCode.Substring(4, 2);
        //            var minor = fullCode.Substring(6, 3);
        //            var subMinor = fullCode.Substring(9, 2);
        //            var detail = fullCode.Substring(11, 3);
        //            var obj = fullCode.Substring(14, 3);

        //            // ================= ADD TO LIST =================

        //            majorList.Add(new MajorHead
        //            {
        //                MajorAccCode = major,
        //                MajorAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            subMajorList.Add(new SubMajorHead
        //            {
        //                MajorAccCode = major,
        //                SubMajorAccCode = subMajor,
        //                SubMajorAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            minorList.Add(new MinorHead
        //            {
        //                MinorAccCode = minor,
        //                MinorAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            subMinorList.Add(new SubMinorHead
        //            {
        //                MinorAccCode = minor,
        //                SubMinorAccCode = subMinor,
        //                SubMinorAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            detailList.Add(new DetailHead
        //            {
        //                DetailAccCode = detail,
        //                DetailAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            objectList.Add(new ObjectClassification
        //            {
        //                ObjectAccCode = obj,
        //                ObjectAccDesc = desc,
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });

        //            headMasterList.Add(new HeadOfAccount
        //            {
        //                MajorAccCode = major,
        //                SubMajorAccCode = subMajor,
        //                MinorAccCode = minor,
        //                SubMinorAccCode = subMinor,
        //                DetailAccCode = detail,
        //                ObjectAccCode = obj,
        //                HeadDesc = desc,
        //                PartyEmpRequired = "N",
        //                PartyLedgToPrint = "N",
        //                TdsDeducted = "N",
        //                AdvanceHead = "N",
        //                BankCash = "N",
        //                MonthlyCarryFwd = "N",
        //                ModificationDate = DateTime.UtcNow,
        //                UserCode = "SYSTEM"
        //            });
        //        }

        //        // ================= REMOVE DUPLICATES IN MEMORY =================

        //        majorList = majorList.GroupBy(x => x.MajorAccCode).Select(g => g.First()).ToList();
        //        subMajorList = subMajorList
        //            .GroupBy(x => x.MajorAccCode + x.SubMajorAccCode)
        //            .Select(g => g.First()).ToList();
        //        minorList = minorList.GroupBy(x => x.MinorAccCode).Select(g => g.First()).ToList();
        //        subMinorList = subMinorList
        //            .GroupBy(x => x.MinorAccCode + x.SubMinorAccCode)
        //            .Select(g => g.First()).ToList();
        //        detailList = detailList.GroupBy(x => x.DetailAccCode).Select(g => g.First()).ToList();
        //        objectList = objectList.GroupBy(x => x.ObjectAccCode).Select(g => g.First()).ToList();

        //        // ================= INSERT =================

        //        await _context.MajorHeads.AddRangeAsync(majorList, cancellationToken);
        //        await _context.SubMajorHeads.AddRangeAsync(subMajorList, cancellationToken);
        //        await _context.MinorHeads.AddRangeAsync(minorList, cancellationToken);
        //        await _context.SubMinorHeads.AddRangeAsync(subMinorList, cancellationToken);
        //        await _context.DetailHeads.AddRangeAsync(detailList, cancellationToken);
        //        await _context.ObjectClassifications.AddRangeAsync(objectList, cancellationToken);
        //        await _context.HeadOfAccounts.AddRangeAsync(headMasterList, cancellationToken);

        //        return new Response
        //        {
        //            IsSucced = true,
        //            Message = $"{headMasterList.Count} Head Of Account records uploaded successfully"
        //        };
        //    }
    }
}