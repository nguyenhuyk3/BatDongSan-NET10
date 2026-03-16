using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDotMoBanDieuChinh
{
    public string MaPhieu { get; set; } = null!;

    public string MaPhieuMoBan { get; set; } = null!;

    public DateTime NgayLap { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public int? SoLanChinh { get; set; }

    public DateTime? NgayMoBan { get; set; }

    public string? MaBlock { get; set; }

    public int? SoLuongCanMoBan { get; set; }

    public int? SoLuongCanBooking { get; set; }

    public double? TyLeChuyenDoi { get; set; }

    public double GiaBan { get; set; }

    public string NoiDung { get; set; } = null!;

    public bool? TrangThai { get; set; }

    public virtual DaDotMoBan MaPhieuMoBanNavigation { get; set; } = null!;
}
