using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucChinhSachThanhToan
{
    public string MaPttt { get; set; } = null!;

    public string TenPttt { get; set; } = null!;

    public DateTime NgayLap { get; set; }

    public string NguoiLap { get; set; } = null!;

    public DateOnly NgayApDung { get; set; }

    public string TinhTrangApDung { get; set; } = null!;
}
