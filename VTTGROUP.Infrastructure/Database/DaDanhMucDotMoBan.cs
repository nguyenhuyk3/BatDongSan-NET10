using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucDotMoBan
{
    public string MaDotMoBan { get; set; } = null!;

    public string? TenDotMoBan { get; set; }

    public string? MaDuAn { get; set; }

    public int? ThuTuHienThi { get; set; }

    public string? MaMau { get; set; }

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public bool? IsKhac { get; set; }

    public bool? IsKichHoat { get; set; }
}
