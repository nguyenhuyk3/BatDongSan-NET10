using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhKeHoachBanHang
{
    public string MaPhieuKh { get; set; } = null!;

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? NoiDung { get; set; }

    public decimal? DonGiaTb { get; set; }

    public decimal? DoanhThuDuKien { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }
}
