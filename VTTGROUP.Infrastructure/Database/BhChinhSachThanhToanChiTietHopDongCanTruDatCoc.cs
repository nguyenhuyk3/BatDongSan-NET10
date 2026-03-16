using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhChinhSachThanhToanChiTietHopDongCanTruDatCoc
{
    public string MaCstt { get; set; } = null!;

    public int DotTthopDong { get; set; }

    public int DotTtdatCoc { get; set; }
}
