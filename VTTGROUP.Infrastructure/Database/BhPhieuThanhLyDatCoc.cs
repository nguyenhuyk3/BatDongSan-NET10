using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuThanhLyDatCoc
{
    public string MaPhieuThanhLy { get; set; } = null!;

    public string MaPhieuDatCoc { get; set; } = null!;

    public DateTime NgayThanhLy { get; set; }

    public string NguoiThanhLy { get; set; } = null!;

    public string? LyDoThanhLy { get; set; }

    public string HinhThucThanhLy { get; set; } = null!;

    public string? GhiChu { get; set; }

    public decimal? PhiPhat { get; set; }

    public decimal? TienHoanLai { get; set; }

    public DateTime? NgayLap { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? SoPhieuThanhLy { get; set; }

    public string? MaPhieuDangKy { get; set; }
}
