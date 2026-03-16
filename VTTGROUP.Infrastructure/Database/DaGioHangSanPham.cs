using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaGioHangSanPham
{
    public int Id { get; set; }

    public string MaGioHang { get; set; } = null!;

    public string? MaSanPham { get; set; }

    public string? MaSanGd { get; set; }

    public double? HeSoCanHo { get; set; }

    public double? GiaBan { get; set; }

    public virtual DaGioHang MaGioHangNavigation { get; set; } = null!;
}
