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

        public async Task SaveFileAsync(Stream content, FileType fileType, string fileName, CancellationToken ct)
        {
            var fullPath = FullPath(fileType, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            await using var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);
            await content.CopyToAsync(stream, ct);
        }
        public Stream? OpenRead(FileType fileType, string fileName)
        {
            var fullPath = FullPath(fileType, fileName);
            return File.Exists(fullPath) ? new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
        }
        public void TryDelete(FileType fileType, string fileName)
        {
            try { File.Delete(FullPath(fileType, fileName)); }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        private string FullPath(FileType fileType, string fileName)
        {
            // The name is the server-generated storage key; a path separator would escape the folder.
            if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
                throw new ArgumentException("Invalid stored file name.", nameof(fileName));
            return Path.Combine(_settings.FolderPath, ResolveFolder(fileType), fileName);
        }
        private string ResolveFolder(FileType fileType)
        {
            return fileType switch
            {
                FileType.Employee => _settings.EmployeeFolderPath,
                FileType.Family => _settings.EmployeeFamilyFolderPath,
                FileType.Documents => "Documents",
                FileType.BudgetAttachment => _settings.BudgetAttachmentFolderPath,
                _ => throw new ArgumentOutOfRangeException(nameof(fileType), "Invalid file type")
            };
        }
    }
}