using BBMB.BudgetService.Features.AccountingHeads.MajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.MinorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMajorHeads.Entities;
using BBMB.BudgetService.Features.AccountingHeads.SubMinor.Entities;
using BBMB.BudgetService.Features.DetailHeads.Entities;
using BBMB.BudgetService.Features.Divisions.Entities;
using BBMB.BudgetService.Features.HeadOfAccounts.Entities;
using BBMB.BudgetService.Features.ObjectClassifications.Entities;
using BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads;
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

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);

                // Required for EPPlus (.NET 8/9/10)
                ExcelPackage.License.SetNonCommercialPersonal("BBMB");

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return new Response { IsSucced = false, Message = "Invalid Excel file" };

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                    return new Response { IsSucced = false, Message = "Excel has no data" };

                var type = request.uploadType.Trim().ToLower();

                Response result;

                switch (type)
                {
                    case "majormaster":
                        result = await UploadMajorMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "submajormaster":
                        result = await UploadSubMajorMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "minormaster":
                        result = await UploadMinorMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "subminormaster":
                        result = await UploadSubMinorMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "detailmaster":
                        result = await UploadDetailMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "objectmaster":
                        result = await UploadObjectMaster(worksheet, rowCount, cancellationToken);
                        break;

                    case "headofaccountmaster":
                        result = await UploadHeadOfAccountMaster(worksheet, rowCount, cancellationToken);
                        break;

                    default:
                        return new Response
                        {
                            IsSucced = false,
                            Message = "Invalid UploadType"
                        };
                }

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
                await transaction.RollbackAsync(cancellationToken);

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
            var list = new List<MajorHead>();
            var duplicateCheck = new HashSet<string>();

            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 1].Text.Trim();
                var desc = worksheet.Cells[row, 2].Text.Trim();

                if (code.Length != 4)
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Major code at row {row}"
                    };

                if (!duplicateCheck.Add(code))
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Major code in Excel: {code}"
                    };

                if (await _context.MajorHeads
                    .AnyAsync(x => x.MajorAccCode == code, cancellationToken))
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

            await _context.MajorHeads.AddRangeAsync(list, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response
            {
                IsSucced = true,
                Message = "Major master uploaded successfully"
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

            // 🔥 Load existing data once
            var existingMajors = await _context.MajorHeads
                .Select(x => x.MajorAccCode)
                .ToListAsync(cancellationToken);

            var existingSubMajors = await _context.SubMajorHeads
                .Select(x => x.MajorAccCode + x.SubMajorAccCode)
                .ToListAsync(cancellationToken);

            var majorSet = new HashSet<string>(existingMajors);
            var subMajorSet = new HashSet<string>(existingSubMajors);

            for (int row = 2; row <= rowCount; row++)
            {
                var major = worksheet.Cells[row, 1].Text?.Trim();
                var subMajor = worksheet.Cells[row, 2].Text?.Trim();
                var desc = worksheet.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrWhiteSpace(major) ||
                    string.IsNullOrWhiteSpace(subMajor) ||
                    string.IsNullOrWhiteSpace(desc))
                    return new Response { IsSucced = false, Message = $"Empty data at row {row}" };

                if (major.Length != 4 || subMajor.Length != 2)
                    return new Response { IsSucced = false, Message = $"Invalid SubMajor format at row {row}" };

                var key = major + subMajor;

                if (!excelDuplicateCheck.Add(key))
                    return new Response { IsSucced = false, Message = $"Duplicate SubMajor in Excel: {key}" };

                if (!majorSet.Contains(major))
                    return new Response { IsSucced = false, Message = $"Major does not exist: {major}" };

                if (subMajorSet.Contains(key))
                    return new Response { IsSucced = false, Message = $"SubMajor already exists: {key}" };

                list.Add(new SubMajorHead
                {
                    MajorAccCode = major,
                    SubMajorAccCode = subMajor,
                    SubMajorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            await _context.SubMajorHeads.AddRangeAsync(list, cancellationToken);

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

            var existingMinors = await _context.MinorHeads
                .Select(x => x.MinorAccCode)
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<string>(existingMinors);

            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 1].Text?.Trim();
                var desc = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrWhiteSpace(code) ||
                    string.IsNullOrWhiteSpace(desc))
                    return new Response { IsSucced = false, Message = $"Empty Minor at row {row}" };

                if (code.Length != 3 || !code.All(char.IsDigit))
                    return new Response { IsSucced = false, Message = $"Invalid Minor at row {row}" };

                if (!excelDuplicateCheck.Add(code))
                    return new Response { IsSucced = false, Message = $"Duplicate Minor in Excel: {code}" };

                if (existingSet.Contains(code))
                    return new Response { IsSucced = false, Message = $"Minor already exists: {code}" };

                list.Add(new MinorHead
                {
                    MinorAccCode = code,
                    MinorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            await _context.MinorHeads.AddRangeAsync(list, cancellationToken);

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

            var existingMinors = await _context.MinorHeads
                .Select(x => x.MinorAccCode)
                .ToListAsync(cancellationToken);

            var existingSubMinors = await _context.SubMinorHeads
                .Select(x => x.MinorAccCode + x.SubMinorAccCode)
                .ToListAsync(cancellationToken);

            var minorSet = new HashSet<string>(existingMinors);
            var subMinorSet = new HashSet<string>(existingSubMinors);

            for (int row = 2; row <= rowCount; row++)
            {
                var minor = worksheet.Cells[row, 1].Text?.Trim();
                var subMinor = worksheet.Cells[row, 2].Text?.Trim();
                var desc = worksheet.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrWhiteSpace(minor) ||
                    string.IsNullOrWhiteSpace(subMinor) ||
                    string.IsNullOrWhiteSpace(desc))
                    return new Response { IsSucced = false, Message = $"Empty SubMinor at row {row}" };

                if (minor.Length != 3 || subMinor.Length != 2)
                    return new Response { IsSucced = false, Message = $"Invalid SubMinor format at row {row}" };

                var key = minor + subMinor;

                if (!excelDuplicateCheck.Add(key))
                    return new Response { IsSucced = false, Message = $"Duplicate SubMinor in Excel: {key}" };

                if (!minorSet.Contains(minor))
                    return new Response { IsSucced = false, Message = $"Minor does not exist: {minor}" };

                if (subMinorSet.Contains(key))
                    return new Response { IsSucced = false, Message = $"SubMinor already exists: {key}" };

                list.Add(new SubMinorHead
                {
                    MinorAccCode = minor,
                    SubMinorAccCode = subMinor,
                    SubMinorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            await _context.SubMinorHeads.AddRangeAsync(list, cancellationToken);

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

            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 1].Text.Trim();
                var desc = worksheet.Cells[row, 2].Text.Trim();

                if (code.Length != 3)
                    return new Response { IsSucced = false, Message = $"Invalid Detail code at row {row}" };

                if (await _context.DetailHeads.AnyAsync(x => x.DetailAccCode == code, cancellationToken))
                    return new Response { IsSucced = false, Message = $"Detail already exists: {code}" };

                list.Add(new DetailHead
                {
                    DetailAccCode = code,
                    DetailAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            await _context.DetailHeads.AddRangeAsync(list, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response { IsSucced = true, Message = "Detail master uploaded successfully" };
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
            var duplicateExcelCheck = new HashSet<string>();

            // 🔥 Load existing object codes once (Performance optimized)
            var existingCodes = await _context.ObjectClassifications
                .Select(x => x.ObjectAccCode)
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<string>(existingCodes);

            for (int row = 2; row <= rowCount; row++)
            {
                var code = worksheet.Cells[row, 1].Text?.Trim();
                var desc = worksheet.Cells[row, 2].Text?.Trim();

                // ================= EMPTY CHECK =================
                if (string.IsNullOrWhiteSpace(code) ||
                    string.IsNullOrWhiteSpace(desc))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Object code or description is empty at row {row}"
                    };
                }

                // ================= LENGTH & NUMERIC CHECK =================
                if (code.Length != 3 || !code.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Object code at row {row}. Must be 3 digits."
                    };
                }

                // ================= DUPLICATE CHECK (EXCEL) =================
                if (!duplicateExcelCheck.Add(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Object code in Excel: {code}"
                    };
                }

                // ================= DUPLICATE CHECK (DATABASE) =================
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

            // ================= BULK INSERT =================
            await _context.ObjectClassifications.AddRangeAsync(list, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

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

            var majors = new HashSet<string>(
                await _context.MajorHeads.Select(x => x.MajorAccCode).ToListAsync(cancellationToken));

            var subMajors = new HashSet<string>(
                await _context.SubMajorHeads
                    .Select(x => x.MajorAccCode + x.SubMajorAccCode)
                    .ToListAsync(cancellationToken));

            var minors = new HashSet<string>(
                await _context.MinorHeads.Select(x => x.MinorAccCode).ToListAsync(cancellationToken));

            var subMinors = new HashSet<string>(
                await _context.SubMinorHeads
                    .Select(x => x.MinorAccCode + x.SubMinorAccCode)
                    .ToListAsync(cancellationToken));

            var details = new HashSet<string>(
                await _context.DetailHeads.Select(x => x.DetailAccCode).ToListAsync(cancellationToken));

            var objects = new HashSet<string>(
                await _context.ObjectClassifications.Select(x => x.ObjectAccCode).ToListAsync(cancellationToken));

            for (int row = 2; row <= rowCount; row++)
            {
                var fullCode = worksheet.Cells[row, 1].Text?.Trim();
                var desc = worksheet.Cells[row, 2].Text?.Trim();

                if (string.IsNullOrWhiteSpace(fullCode) ||
                    string.IsNullOrWhiteSpace(desc))
                    return new Response { IsSucced = false, Message = $"Empty HeadOfAccount at row {row}" };

                if (fullCode.Length != 17 || !fullCode.All(char.IsDigit))
                    return new Response { IsSucced = false, Message = $"Invalid 17 digit code at row {row}" };

                if (!excelDuplicateCheck.Add(fullCode))
                    return new Response { IsSucced = false, Message = $"Duplicate in Excel: {fullCode}" };

                var major = fullCode.Substring(0, 4);
                var subMajor = fullCode.Substring(4, 2);
                var minor = fullCode.Substring(6, 3);
                var subMinor = fullCode.Substring(9, 2);
                var detail = fullCode.Substring(11, 3);
                var obj = fullCode.Substring(14, 3);

                if (!majors.Contains(major)) return new Response { IsSucced = false, Message = $"Major missing: {major}" };
                if (!subMajors.Contains(major + subMajor)) return new Response { IsSucced = false, Message = $"SubMajor missing: {major}{subMajor}" };
                if (!minors.Contains(minor)) return new Response { IsSucced = false, Message = $"Minor missing: {minor}" };
                if (!subMinors.Contains(minor + subMinor)) return new Response { IsSucced = false, Message = $"SubMinor missing: {minor}{subMinor}" };
                if (!details.Contains(detail)) return new Response { IsSucced = false, Message = $"Detail missing: {detail}" };
                if (!objects.Contains(obj)) return new Response { IsSucced = false, Message = $"Object missing: {obj}" };

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

            await _context.HeadOfAccounts.AddRangeAsync(list, cancellationToken);

            return new Response
            {
                IsSucced = true,
                Message = $"{list.Count} HeadOfAccount records uploaded successfully"
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