using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucView
{
    public string MaView { get; set; } = null!;

    public string TenView { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public decimal? HeSoView { get; set; }

    public int? ThuTuHienThi { get; set; }
}
