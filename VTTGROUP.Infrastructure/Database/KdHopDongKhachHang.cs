using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdHopDongKhachHang
{
    public string MaHopDong { get; set; } = null!;

    public string MaKhachHang { get; set; } = null!;

    public string? IdlanDieuChinhKh { get; set; }

    public bool? IskhdaiDien { get; set; }
}
