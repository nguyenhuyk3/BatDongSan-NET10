using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDuyetGium
{
    public string MaPhieu { get; set; } = null!;

    public string? MaDuAn { get; set; }

    public string? MaDotMoBan { get; set; }

    public decimal? GiaBanKeHoach { get; set; }

    public decimal? GiaBanThucTe { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? MaPhieuKh { get; set; }

    public string? NoiDung { get; set; }

    public decimal? TyLeChuyenDoi { get; set; }
}
