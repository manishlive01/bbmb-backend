namespace BBMB.BudgetService.Shared.Settings
{
    public class FileSettings
    {
        public string FileServerUrl { get; set; } = null!;
        public string FolderPath { get; set; } = null!;
        public string EmployeeFolderPath { get; set; } = null!;
        public string EmployeeFamilyFolderPath { get; set; } = null!;
        /// <summary>Folder of T-22 budget attachments under <see cref="FolderPath"/> (design §4.8).</summary>
        public string BudgetAttachmentFolderPath { get; set; } = "BudgetAttachments";
    }
}
