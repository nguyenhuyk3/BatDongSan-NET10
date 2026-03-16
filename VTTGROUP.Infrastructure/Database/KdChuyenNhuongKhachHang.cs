using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdChuyenNhuongKhachHang
{
    public long Id { get; set; }

    public string MaChuyenNhuong { get; set; } = null!;

    public string MaKhachHang { get; set; } = null!;

    public string IdlanDieuChinhKh { get; set; } = null!;

    public int VaiTro { get; set; }

    public int? Stt { get; set; }

    public bool IsDaiDien { get; set; }
}
