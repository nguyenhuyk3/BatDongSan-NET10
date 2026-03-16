using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucTang
{
    public string MaDuAn { get; set; } = null!;

    public string MaBlock { get; set; } = null!;

    public string MaTang { get; set; } = null!;

    public string? TenTang { get; set; }

    public decimal? HeSoTang { get; set; }

    public int? Stttang { get; set; }

    public string? HinhAnh { get; set; }
}
