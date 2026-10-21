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

        /// <summary>
        /// Stores <paramref name="content"/> under exactly <paramref name="fileName"/> (the T-22 storage key, never
        /// reused, so no upload overwrites another; design §4.8 step 5). Refuses to overwrite an existing file.
        /// </summary>
        Task SaveFileAsync(Stream content, FileType fileType, string fileName, CancellationToken ct);

        /// <summary>Opens a stored file for reading, or null when it is absent.</summary>
        Stream? OpenRead(FileType fileType, string fileName);

        /// <summary>Best-effort removal of a file whose database row was not written (design §8.2 upload row).</summary>
        void TryDelete(FileType fileType, string fileName);
    }
}
