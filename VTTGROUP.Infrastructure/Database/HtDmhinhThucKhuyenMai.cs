using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDmhinhThucKhuyenMai
{
    public string MaHinhThucKm { get; set; } = null!;

    public string? TenHinhThucKm { get; set; }

    public bool? IsCkTyLe { get; set; }
}
