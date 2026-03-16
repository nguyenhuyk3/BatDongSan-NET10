using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucHuong
{
    public string MaDuAn { get; set; } = null!;

    public string MaHuong { get; set; } = null!;

    public string? TenHuong { get; set; }

    public decimal? HeSo { get; set; }
}
