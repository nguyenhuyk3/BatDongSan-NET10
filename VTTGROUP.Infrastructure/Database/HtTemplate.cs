using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtTemplate
{
    public string MaTemplate { get; set; } = null!;

    public string? TenTemplate { get; set; }

    public string? MaDuAn { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? TieuDe { get; set; }

    public string? NoiDung { get; set; }

    public string? SoTemplate { get; set; }
}
