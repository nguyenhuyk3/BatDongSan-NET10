using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDanhMucTinhTrangSanPham
{
    public string MaTinhTrang { get; set; } = null!;

    public string? TenTinhTrang { get; set; }

    public string? MaMau { get; set; }
}
