using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuGiuChoStt
{
    public string MaPhieuGiuCho { get; set; } = null!;

    public string MaDotBanHang { get; set; } = null!;

    public int? SoTtbooking { get; set; }
}
