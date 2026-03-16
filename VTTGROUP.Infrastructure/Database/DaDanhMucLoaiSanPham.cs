using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiSanPham
{
    public string MaLoaiSanPham { get; set; } = null!;

    public string? TenLoaiSanPham { get; set; }
}
