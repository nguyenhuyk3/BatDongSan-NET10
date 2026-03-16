using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDanhMucHinhThucThanhToan
{
    public string MaHttt { get; set; } = null!;

    public string? TenHttt { get; set; }

    public bool? IsCanTruNguonKhac { get; set; }
}
