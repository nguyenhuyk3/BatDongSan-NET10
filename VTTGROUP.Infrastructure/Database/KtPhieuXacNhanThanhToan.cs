using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtPhieuXacNhanThanhToan
{
    public string MaPhieu { get; set; } = null!;

    public string? SoChungTu { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public DateTime? NgayHachToan { get; set; }

    public string? MaDuAn { get; set; }

    /// <summary>
    /// T: Thu; C: Chi
    /// </summary>
    public string? LoaiPhieu { get; set; }

    public string? HinhThucThanhToan { get; set; }

    public string? MaKhachHang { get; set; }

    public string? TenKhachHang { get; set; }

    public string? NoiDung { get; set; }

    public bool? IsXacNhan { get; set; }
}
