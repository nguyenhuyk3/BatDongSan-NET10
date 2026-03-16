using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Model.Email
{
    public class EmailSendModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool TrangThai { get; set; } // false = chưa gửi
        public string? NguoiLap { get; set; }
        public DateTime NgayLap { get; set; }
        public DateTime? NgayGui { get; set; }
        public string? NoiDungLoi { get; set; }
    }

    public class EmailMessageModal
    {
        public List<string> To { get; set; } = new();
        public string Subject { get; set; } = string.Empty;
        public string BodyHtml { get; set; } = string.Empty;
        public List<(byte[] FileBytes, string FileName)>? Attachments { get; set; }
    }

    public class EmailSettingsModal
    {
        public string SmtpServer { get; set; } = default!;
        public int SmtpPort { get; set; }
        public string SenderName { get; set; } = default!;
        public string SenderEmail { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
    }
    public class OfficeOptions
    {
        public string? SofficePath { get; set; }
    }
}
