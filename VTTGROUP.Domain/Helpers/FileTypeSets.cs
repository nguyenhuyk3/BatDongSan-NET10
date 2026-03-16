namespace VTTGROUP.Domain.Helpers
{
    public static class FileTypeSets
    {
        public static readonly string[] Image = new[]
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".tiff", ".svg"
        };

        public static readonly string[] Document = new[]
        {
            ".pdf", ".docx", ".xlsx", ".pptx",".doc", ".xls", ".txt", ".ppt", ".pptx", ".csv"
        };

        public static readonly string[] Media = new[]
        {
            ".mp3", ".wav", ".ogg", ".aac", ".midi", ".mid", ".mp4", ".webm", ".ogv", ".avi", ".mov", ".mpeg", ".mpg"
        };

        public static readonly string[] Archive = new[]
        {
            ".zip", ".rar", ".7z", ".tar", ".gz"
        };

        public static readonly string[] ImgAndDoc = Image.Concat(Document).ToArray();

        public static readonly string[] All = Image.Concat(Document).Concat(Media).Concat(Archive).ToArray();
    }
}
