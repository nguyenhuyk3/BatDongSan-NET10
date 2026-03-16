using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtBuocDuyet
{
    public string MaBuocDuyet { get; set; } = null!;

    public string? TenBuocDuyet { get; set; }

    public string? TenHienThi { get; set; }

    public string? GhiChu { get; set; }

    public int IdbuocDuyet { get; set; }
}
