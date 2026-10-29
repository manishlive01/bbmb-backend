using MediatR;
using Microsoft.AspNetCore.Http;
using BBMB.BudgetService.Shared.ResponseDTOs;

namespace BBMB.BudgetService.Features.BulkExcelUploads.BulkUploads
{
    public sealed record BulkUploadCommand(
        IFormFile File,
        string uploadType
    ) : IRequest<Response>;
}