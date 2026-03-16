using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdPhuLucHopDong
{
    public string MaPhuLuc { get; set; } = null!;

    public string? SoPhuLuc { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? MaHopDong { get; set; }

    public string? MaQuiTrinhDuyet { get; set; }

    public string? TrangThaiDuyet { get; set; }

    public DateTime? NgayKyPl { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaTriCk { get; set; }

    public decimal? GiaBanSauCk { get; set; }

    public decimal? GiaBanTruocThue { get; set; }

    public decimal? GiaBanSauThue { get; set; }

    public string? MaMauIn { get; set; }

    public string? NoiDung { get; set; }

    public string? MaCstt { get; set; }
}
