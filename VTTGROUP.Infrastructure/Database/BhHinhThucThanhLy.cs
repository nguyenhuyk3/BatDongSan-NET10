using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhHinhThucThanhLy
{
    public string MaHttl { get; set; } = null!;

    public string? TenHttl { get; set; }

    public bool? IsDatCocLai { get; set; }
}
