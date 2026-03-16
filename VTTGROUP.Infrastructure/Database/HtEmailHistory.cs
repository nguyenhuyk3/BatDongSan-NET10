using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtEmailHistory
{
    public int Id { get; set; }

    public string? TieuDe { get; set; }

    public string? Email { get; set; }

    public string? NoiDung { get; set; }

    public bool? TrangThai { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public DateTime? NgayGui { get; set; }

    public string? NoiDungLoi { get; set; }

    public string? FileDinhKem { get; set; }
}
