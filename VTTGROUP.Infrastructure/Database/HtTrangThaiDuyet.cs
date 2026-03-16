using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtTrangThaiDuyet
{
    public string MaTrangThai { get; set; } = null!;

    public string? TenTrangThai { get; set; }

    public string? Icon { get; set; }

    public string? Mau { get; set; }
}
