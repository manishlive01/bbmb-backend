using BBMB.BudgetService.Shared.Enums;

namespace BBMB.BudgetService.Shared.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(
            IFormFile file,
            FileType fileType,
            string? subFolder,
            CancellationToken ct);
    }
}
