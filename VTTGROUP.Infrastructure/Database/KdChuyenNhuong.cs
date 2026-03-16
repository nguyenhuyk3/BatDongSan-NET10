using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdChuyenNhuong
{
    public string MaChuyenNhuong { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string MaHopDong { get; set; } = null!;

    public string MaSanPham { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public DateTime? NgayChuyenNhuong { get; set; }

    public string NguoiLap { get; set; } = null!;

    public decimal? GiaTriCanHo { get; set; }

    public decimal? GiaTriDaThanhToan { get; set; }

    public decimal? PhiBaoTri { get; set; }

    public decimal? PhiBaoTriDaTt { get; set; }

    public string? NoiDung { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }
}
