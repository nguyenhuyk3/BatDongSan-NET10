using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdPhieuDeNghiHoanTienBooking
{
    public string MaPhieu { get; set; } = null!;

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaSanGiaoDich { get; set; }

    public string? NoiDung { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }
}
