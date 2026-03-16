using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdPhieuDeNghiHoanTienBookingSoPhieuBooking
{
    public string MaPhieuHoanTien { get; set; } = null!;

    public string MaPhieuBooking { get; set; } = null!;

    public string MaPhieuTongHopThu { get; set; } = null!;

    public decimal? SoTien { get; set; }

    public string? GhiChu { get; set; }
}
