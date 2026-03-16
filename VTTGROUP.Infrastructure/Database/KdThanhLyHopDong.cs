using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdThanhLyHopDong
{
    public string MaPhieuTl { get; set; } = null!;

    public string? SoPhieuTl { get; set; }

    public string? MaHopDong { get; set; }

    public DateTime NgayThanhLy { get; set; }

    public string NguoiThanhLy { get; set; } = null!;

    public string? NoiDung { get; set; }

    public string HinhThucThanhLy { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public decimal? SoTienDaThu { get; set; }

    public decimal? SoTienPhiBaoTriDaThu { get; set; }

    public decimal? TyLeViPham { get; set; }

    public decimal? SoTienViPhamHopDong { get; set; }

    public decimal? SoTienHoanTra { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }
}
