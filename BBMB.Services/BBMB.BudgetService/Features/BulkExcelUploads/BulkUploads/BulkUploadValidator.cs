using BBMB.BudgetService.Features.BulkUploads;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads
{
    public class BulkUploadValidator : AbstractValidator<BulkUploadCommand>
    {
        public BulkUploadValidator()
        {
            // Validate Type
            //RuleFor(x => x.Type)
            //    .IsInEnum()
            //    .WithMessage("Invalid BulkUploadType.");

            // Validate File not null
            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("File is required.");

            // Validate File not empty
            RuleFor(x => x.File.Length)
                .GreaterThan(0)
                .WithMessage("Uploaded file is empty.");

            RuleFor(x => x.File.FileName)
             .Must(BeValidExcelFile)
             .WithMessage("Only .xlsx, .xls or .csv files are allowed.");

        }

        private bool BeValidExcelFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension == ".xlsx"
                || extension == ".xls"
                || extension == ".csv";
        }
    }
}