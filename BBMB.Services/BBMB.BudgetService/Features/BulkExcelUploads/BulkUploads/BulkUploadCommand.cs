using BBMB.BudgetService.Shared.ResponseDTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace BBMB.BudgetService.Features.BulkUploads
{
    public sealed record BulkUploadCommand(
        IFormFile File,
        string Type
    ) : IRequest<Response>;
}