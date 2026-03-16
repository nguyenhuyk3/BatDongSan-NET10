using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaLoaiDienTich
{
    public string MaDuAn { get; set; } = null!;

    public string MaLoaiDt { get; set; } = null!;

    public string? TenLoaiDt { get; set; }

    public decimal? HeSo { get; set; }
}
