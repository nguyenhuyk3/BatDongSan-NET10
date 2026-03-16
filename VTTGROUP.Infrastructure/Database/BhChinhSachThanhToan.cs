using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhChinhSachThanhToan
{
    public string MaCstt { get; set; } = null!;

    public string TenCstt { get; set; } = null!;

    public string? MaDuAn { get; set; }

    public decimal? TyLeChietKhau { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? NoiDung { get; set; }

    public bool? IsXacNhan { get; set; }
}
