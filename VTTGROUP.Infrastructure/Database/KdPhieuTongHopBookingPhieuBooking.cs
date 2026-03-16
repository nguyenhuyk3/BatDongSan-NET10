using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdPhieuTongHopBookingPhieuBooking
{
    public string MaPhieuTh { get; set; } = null!;

    public string MaBooking { get; set; } = null!;

    public decimal? SoTien { get; set; }

    public string? GhiChu { get; set; }
}
