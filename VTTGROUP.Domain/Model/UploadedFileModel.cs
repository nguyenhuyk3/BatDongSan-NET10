using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace VTTGROUP.Domain.Model
{
    public class UploadedFileModel
    {
        public int? Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileNameSave { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public string Base64 { get; set; } = string.Empty;
        public string FolderUrl { get; set; } = string.Empty;
        public string FileSize { get; set; } = string.Empty;
        public string FullDomain { get; set; } = string.Empty;
        public bool IsVideo => ContentType.StartsWith("video/");
    }
}
