using BBMB.BudgetService.Shared.Enums;
using BBMB.BudgetService.Shared.Settings;
using Microsoft.Extensions.Options;

namespace BBMB.BudgetService.Shared.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly FileSettings _settings;

        public FileStorageService(IOptions<FileSettings> options)
        {
            _settings = options.Value;
        }

        public async Task<string> SaveFileAsync(
            IFormFile file,
            FileType fileType,
            string? subFolder,
            CancellationToken ct)
        {
            var folderType = ResolveFolder(fileType);

            var rootPath = _settings.FolderPath;

            var folderPath = Path.Combine(rootPath, folderType);

            if (!string.IsNullOrWhiteSpace(subFolder))
                folderPath = Path.Combine(folderPath, subFolder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream, ct);

            var relativePath = Path.Combine(folderType, subFolder ?? "", fileName)
                .Replace("\\", "/");

            return "/" + relativePath;
        }

        private string ResolveFolder(FileType fileType)
        {
            return fileType switch
            {
                FileType.Employee => _settings.EmployeeFolderPath,
                FileType.Family => _settings.EmployeeFamilyFolderPath,
                FileType.Documents => "Documents",
                _ => throw new ArgumentOutOfRangeException(nameof(fileType), "Invalid file type")
            };
        }
    }
}