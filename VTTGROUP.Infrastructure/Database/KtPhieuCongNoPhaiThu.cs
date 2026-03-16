using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtPhieuCongNoPhaiThu
{
    public string MaPhieu { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public string? DuAn { get; set; }

    public string? MaChungTu { get; set; }

    public string? IdChungTu { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? HanThanhToan { get; set; }

    public string? MaKhachHang { get; set; }

    public string? TenKhachHang { get; set; }

    public decimal? SoTien { get; set; }

    public string? MaCongViec { get; set; }

    public string? MaDoiTuong { get; set; }
}
