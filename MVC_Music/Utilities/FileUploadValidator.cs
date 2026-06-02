namespace MVC_Music.Utilities
{
    public static class FileUploadValidator
    {
        public const long MaxDocumentBytes = 10 * 1024 * 1024; // 10 MB
        public const long MaxPictureBytes = 5 * 1024 * 1024;  // 5 MB

        private static readonly HashSet<string> AllowedDocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".txt", ".png", ".jpg", ".jpeg", ".webp"
        };

        private static readonly HashSet<string> AllowedDocumentMimePrefixes =
        [
            "application/pdf",
            "application/msword",
            "application/vnd.",
            "text/plain",
            "image/"
        ];

        public static bool IsAllowedDocument(IFormFile file, out string? errorMessage)
        {
            errorMessage = null;
            if (file.Length <= 0)
            {
                errorMessage = "The uploaded file is empty.";
                return false;
            }

            if (file.Length > MaxDocumentBytes)
            {
                errorMessage = $"Documents cannot exceed {MaxDocumentBytes / (1024 * 1024)} MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension) || !AllowedDocumentExtensions.Contains(extension))
            {
                errorMessage = "File type is not allowed.";
                return false;
            }

            if (!string.IsNullOrEmpty(file.ContentType)
                && !AllowedDocumentMimePrefixes.Any(p => file.ContentType.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                errorMessage = "File content type is not allowed.";
                return false;
            }

            return true;
        }

        public static bool IsAllowedImage(IFormFile file, out string? errorMessage)
        {
            errorMessage = null;
            if (file.Length <= 0)
            {
                errorMessage = "The uploaded image is empty.";
                return false;
            }

            if (file.Length > MaxPictureBytes)
            {
                errorMessage = $"Images cannot exceed {MaxPictureBytes / (1024 * 1024)} MB.";
                return false;
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Only image files are allowed.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(extension)
                || !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Image file type is not allowed.";
                return false;
            }

            return true;
        }
    }
}
