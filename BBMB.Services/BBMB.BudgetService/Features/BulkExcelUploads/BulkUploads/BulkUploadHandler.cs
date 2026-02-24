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
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Uploaded file is empty"
                };
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return new Response
                {
                    IsSucced = false,
                    Message = "Upload type is required"
                };
            }

            var normalizedType = request.Type.Trim().ToLower();

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                using var stream = new MemoryStream();
                await request.File.CopyToAsync(stream, cancellationToken);

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                if (worksheet == null)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Invalid Excel file"
                    };
                }

                var rowCount = worksheet.Dimension?.Rows ?? 0;

                if (rowCount < 2)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = "Excel file contains no data"
                    };
                }

                // 🔥 STRING BASED SWITCH
                switch (normalizedType)
                {
                    case "headofaccount":
                        var hoaResult = await UploadHeadOfAccounts(
                            worksheet,
                            rowCount,
                            cancellationToken);

                        if (!hoaResult.IsSucced)
                            return hoaResult;

                        break;

                    case "division":
                        var divisionResult = await UploadDivisions(
                            worksheet,
                            rowCount,
                            cancellationToken);

                        if (!divisionResult.IsSucced)
                            return divisionResult;

                        break;

                    default:
                        return new Response
                        {
                            IsSucced = false,
                            Message = "Invalid upload type. Allowed values: headofaccount, division"
                        };
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return new Response
                {
                    IsSucced = true,
                    Message = "Bulk upload completed successfully"
                };
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
        // HEAD OF ACCOUNT - FULL SAFE INSERT
        // ============================================================
        private async Task<Response> UploadHeadOfAccounts(
      ExcelWorksheet worksheet,
      int rowCount,
      CancellationToken cancellationToken)
        {
            var majorList = new List<MajorHead>();
            var subMajorList = new List<SubMajorHead>();
            var minorList = new List<MinorHead>();
            var subMinorList = new List<SubMinorHead>();
            var detailList = new List<DetailHead>();
            var objectList = new List<ObjectClassification>();
            var headMasterList = new List<HeadOfAccount>();

            for (int row = 2; row <= rowCount; row++)
            {
                var major = worksheet.Cells[row, 1].Text?.Trim();
                var subMajor = worksheet.Cells[row, 2].Text?.Trim();
                var minor = worksheet.Cells[row, 3].Text?.Trim();
                var subMinor = worksheet.Cells[row, 4].Text?.Trim();
                var detail = worksheet.Cells[row, 5].Text?.Trim();
                var obj = worksheet.Cells[row, 6].Text?.Trim();
                var desc = worksheet.Cells[row, 7].Text?.Trim();

                // ================= VALIDATION =================

                if (string.IsNullOrWhiteSpace(major) ||
                    string.IsNullOrWhiteSpace(subMajor) ||
                    string.IsNullOrWhiteSpace(minor) ||
                    string.IsNullOrWhiteSpace(subMinor) ||
                    string.IsNullOrWhiteSpace(detail) ||
                    string.IsNullOrWhiteSpace(obj) ||
                    string.IsNullOrWhiteSpace(desc))
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid or empty data at row {row}"
                    };
                }

                // ✅ Correct 17-digit structure validation
                if (major.Length != 4 ||
                    subMajor.Length != 2 ||
                    minor.Length != 3 ||
                    subMinor.Length != 2 ||
                    detail.Length != 3 ||
                    obj.Length != 3)
                {
                    return new Response
                    {
                        IsSucced = false,
                        Message = $"Invalid Head Of Account format at row {row}. Expected 4-2-3-2-3-3 digit structure."
                    };
                }

                majorList.Add(new MajorHead
                {
                    MajorAccCode = major,
                    MajorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                subMajorList.Add(new SubMajorHead
                {
                    MajorAccCode = major,
                    SubMajorAccCode = subMajor,
                    SubMajorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                minorList.Add(new MinorHead
                {
                    MinorAccCode = minor,
                    MinorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                subMinorList.Add(new SubMinorHead
                {
                    MinorAccCode = minor,
                    SubMinorAccCode = subMinor,
                    SubMinorAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                detailList.Add(new DetailHead
                {
                    DetailAccCode = detail,
                    DetailAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                objectList.Add(new ObjectClassification
                {
                    ObjectAccCode = obj,
                    ObjectAccDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });

                headMasterList.Add(new HeadOfAccount
                {
                    MajorAccCode = major,
                    SubMajorAccCode = subMajor,
                    MinorAccCode = minor,
                    SubMinorAccCode = subMinor,
                    DetailAccCode = detail,
                    ObjectAccCode = obj,
                    HeadDesc = desc,
                    ModificationDate = DateTime.UtcNow,
                    UserCode = "SYSTEM"
                });
            }

            // ================= DUPLICATE CHECK (EXCEL) =================

            var duplicateExcel = headMasterList
                .GroupBy(x => new
                {
                    x.MajorAccCode,
                    x.SubMajorAccCode,
                    x.MinorAccCode,
                    x.SubMinorAccCode,
                    x.DetailAccCode,
                    x.ObjectAccCode
                })
                .Where(g => g.Count() > 1)
                .Select(g =>
                    $"{g.Key.MajorAccCode}" +
                    $"{g.Key.SubMajorAccCode}" +
                    $"{g.Key.MinorAccCode}" +
                    $"{g.Key.SubMinorAccCode}" +
                    $"{g.Key.DetailAccCode}" +
                    $"{g.Key.ObjectAccCode}")
                .ToList();

            if (duplicateExcel.Any())
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Duplicate Head Of Account(s) found in Excel: {string.Join(", ", duplicateExcel)}"
                };
            }

            // ================= DUPLICATE CHECK (DATABASE) =================

            var existingHeads = await _context.HeadOfAccounts
                .Select(x => new
                {
                    x.MajorAccCode,
                    x.SubMajorAccCode,
                    x.MinorAccCode,
                    x.SubMinorAccCode,
                    x.DetailAccCode,
                    x.ObjectAccCode
                })
                .ToListAsync(cancellationToken);

            var duplicateDb = headMasterList
                .Where(x => existingHeads.Any(e =>
                    e.MajorAccCode == x.MajorAccCode &&
                    e.SubMajorAccCode == x.SubMajorAccCode &&
                    e.MinorAccCode == x.MinorAccCode &&
                    e.SubMinorAccCode == x.SubMinorAccCode &&
                    e.DetailAccCode == x.DetailAccCode &&
                    e.ObjectAccCode == x.ObjectAccCode))
                .Select(x =>
                    $"{x.MajorAccCode}" +
                    $"{x.SubMajorAccCode}" +
                    $"{x.MinorAccCode}" +
                    $"{x.SubMinorAccCode}" +
                    $"{x.DetailAccCode}" +
                    $"{x.ObjectAccCode}")
                .Distinct()
                .ToList();

            if (duplicateDb.Any())
            {
                return new Response
                {
                    IsSucced = false,
                    Message = $"Head Of Account already exists in database: {string.Join(", ", duplicateDb)}"
                };
            }

            // ================= INSERT =================

            await _context.MajorHeads.AddRangeAsync(majorList, cancellationToken);
            await _context.SubMajorHeads.AddRangeAsync(subMajorList, cancellationToken);
            await _context.MinorHeads.AddRangeAsync(minorList, cancellationToken);
            await _context.SubMinorHeads.AddRangeAsync(subMinorList, cancellationToken);
            await _context.DetailHeads.AddRangeAsync(detailList, cancellationToken);
            await _context.ObjectClassifications.AddRangeAsync(objectList, cancellationToken);
            await _context.HeadOfAccounts.AddRangeAsync(headMasterList, cancellationToken);

            return new Response
            {
                IsSucced = true,
                Message = "Head Of Account uploaded successfully"
            };
        }
        // ============================================================
        // DIVISION UPLOAD
        // ============================================================
        private async Task<Response> UploadDivisions(
            ExcelWorksheet worksheet,
            int rowCount,
            CancellationToken cancellationToken)
        {
            var divisionList = new List<Division>();

            //for (int row = 2; row <= rowCount; row++)
            //{
            //    var code = worksheet.Cells[row, 1].Text?.Trim();
            //    var name = worksheet.Cells[row, 2].Text?.Trim();

            //    if (string.IsNullOrWhiteSpace(code) ||
            //        string.IsNullOrWhiteSpace(name))
            //    {
            //        return new Response
            //        {
            //            IsSucced = false,
            //            Message = $"Invalid division data at row {row}"
            //        };
            //    }

            //    divisionList.Add(new Division
            //    {
            //        DivisionCode = code,
            //        DivisionName = name,
            //        ModificationDate = DateTime.UtcNow,
            //        UserCode = "SYSTEM"
            //    });
            //}

            divisionList = divisionList
                .GroupBy(x => x.DivisionCode)
                .Select(g => g.First())
                .ToList();

            await _context.Divisions.AddRangeAsync(divisionList, cancellationToken);

            return new Response { IsSucced = true };
        }
    }
}