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

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                    return new Response { IsSucced = false, Message = "Invalid Excel file" };

                var rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                    return new Response { IsSucced = false, Message = "Excel has no data" };

                var type = request.uploadType.Trim().ToLower();

                switch (type)
                {
                    case "majormaster":
                        return await UploadMajorMaster(worksheet, rowCount, cancellationToken);

                    case "submajormaster":
                        return await UploadSubMajorMaster(worksheet, rowCount, cancellationToken);

                    case "minormaster":
                        return await UploadMinorMaster(worksheet, rowCount, cancellationToken);

                    case "subminormaster":
                        return await UploadSubMinorMaster(worksheet, rowCount, cancellationToken);

                    case "detailmaster":
                        return await UploadDetailMaster(worksheet, rowCount, cancellationToken);

                    case "objectmaster":
                        return await UploadObjectMaster(worksheet, rowCount, cancellationToken);

                    case "headofaccountmaster":
                        return await UploadHeadOfAccountMaster(worksheet, rowCount, cancellationToken);

                    default:
                        return new Response
                        {
                            IsSucced = false,
                            Message = "Invalid UploadType"
                        };
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new Response { IsSucced = false, Message = ex.Message };
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
                    return new Response { IsSucced = false, Message = $"Invalid Major code at row {row}" };

                if (!duplicateCheck.Add(code))
                    return new Response { IsSucced = false, Message = $"Duplicate Major code in Excel: {code}" };

                if (await _context.MajorHeads.AnyAsync(x => x.MajorAccCode == code, cancellationToken))
                    return new Response { IsSucced = false, Message = $"Major already exists: {code}" };

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

            return new Response { IsSucced = true, Message = "Major master uploaded successfully" };
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

            for (int row = 2; row <= rowCount; row++)
            {
                var major = worksheet.Cells[row, 1].Text.Trim();
                var subMajor = worksheet.Cells[row, 2].Text.Trim();
                var desc = worksheet.Cells[row, 3].Text.Trim();

                if (major.Length != 4 || subMajor.Length != 2)
                    return new Response { IsSucced = false, Message = $"Invalid SubMajor format at row {row}" };

                // FK CHECK
                if (!await _context.MajorHeads.AnyAsync(x => x.MajorAccCode == major, cancellationToken))
                    return new Response { IsSucced = false, Message = $"Major does not exist: {major}" };

                if (await _context.SubMajorHeads.AnyAsync(x =>
                    x.MajorAccCode == major &&
                    x.SubMajorAccCode == subMajor, cancellationToken))
                    return new Response { IsSucced = false, Message = $"SubMajor already exists: {major}{subMajor}" };

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
            await _context.SaveChangesAsync(cancellationToken);

            return new Response { IsSucced = true, Message = "SubMajor master uploaded successfully" };
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
            var duplicateExcelCheck = new HashSet<string>();

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
                        Message = $"Minor code or description is empty at row {row}"
                    };
                }

                // ================= LENGTH CHECK =================
                if (code.Length != 3 || !code.All(char.IsDigit))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Minor code at row {row}. Must be 3 digits."
                    };
                }

                // ================= DUPLICATE CHECK (EXCEL) =================
                if (!duplicateExcelCheck.Add(code))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Duplicate Minor code in Excel: {code}"
                    };
                }

                // ================= DUPLICATE CHECK (DATABASE) =================
                if (await _context.MinorHeads
                    .AnyAsync(x => x.MinorAccCode == code, cancellationToken))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Minor already exists in database: {code}"
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

            // ================= BULK INSERT =================
            await _context.MinorHeads.AddRangeAsync(list, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

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

            for (int row = 2; row <= rowCount; row++)
            {
                var minor = worksheet.Cells[row, 1].Text.Trim();
                var subMinor = worksheet.Cells[row, 2].Text.Trim();
                var desc = worksheet.Cells[row, 3].Text.Trim();

                if (minor.Length != 3 || subMinor.Length != 2)
                    return new Response { IsSucced = false, Message = $"Invalid SubMinor format at row {row}" };

                // FK CHECK
                if (!await _context.MinorHeads.AnyAsync(x => x.MinorAccCode == minor, cancellationToken))
                    return new Response { IsSucced = false, Message = $"Minor does not exist: {minor}" };

                if (await _context.SubMinorHeads.AnyAsync(x =>
                    x.MinorAccCode == minor &&
                    x.SubMinorAccCode == subMinor, cancellationToken))
                    return new Response { IsSucced = false, Message = $"SubMinor already exists: {minor}{subMinor}" };

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
            await _context.SaveChangesAsync(cancellationToken);

            return new Response { IsSucced = true, Message = "SubMinor master uploaded successfully" };
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

            for (int row = 2; row <= rowCount; row++)
            {
                var fullCode = worksheet.Cells[row, 1].Text.Trim();
                var desc = worksheet.Cells[row, 2].Text.Trim();

                if (fullCode.Length != 17)
                    return new Response { IsSucced = false, Message = $"Invalid 17 digit code at row {row}" };

                var major = fullCode.Substring(0, 4);
                var subMajor = fullCode.Substring(4, 2);
                var minor = fullCode.Substring(6, 3);
                var subMinor = fullCode.Substring(9, 2);
                var detail = fullCode.Substring(11, 3);
                var obj = fullCode.Substring(14, 3);

                // FK CHECKS
                if (!await _context.MajorHeads.AnyAsync(x => x.MajorAccCode == major))
                    return new Response { IsSucced = false, Message = $"Major missing: {major}" };

                if (!await _context.SubMajorHeads.AnyAsync(x =>
                    x.MajorAccCode == major &&
                    x.SubMajorAccCode == subMajor))
                    return new Response { IsSucced = false, Message = $"SubMajor missing: {major}{subMajor}" };

                if (!await _context.MinorHeads.AnyAsync(x => x.MinorAccCode == minor))
                    return new Response { IsSucced = false, Message = $"Minor missing: {minor}" };

                if (!await _context.SubMinorHeads.AnyAsync(x =>
                    x.MinorAccCode == minor &&
                    x.SubMinorAccCode == subMinor))
                    return new Response { IsSucced = false, Message = $"SubMinor missing: {minor}{subMinor}" };

                if (!await _context.DetailHeads.AnyAsync(x => x.DetailAccCode == detail))
                    return new Response { IsSucced = false, Message = $"Detail missing: {detail}" };

                if (!await _context.ObjectClassifications.AnyAsync(x => x.ObjectAccCode == obj))
                    return new Response { IsSucced = false, Message = $"Object missing: {obj}" };

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
            await _context.SaveChangesAsync(cancellationToken);

            return new Response { IsSucced = true, Message = "HeadOfAccount master uploaded successfully" };
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