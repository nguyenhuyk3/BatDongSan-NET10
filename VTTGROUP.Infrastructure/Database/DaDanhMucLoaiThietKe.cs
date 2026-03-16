using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiThietKe
{
    public string MaLoaiThietKe { get; set; } = null!;

    public string? TenLoaiThietKe { get; set; }

    public string MaDuAn { get; set; } = null!;

    public string? MoTa { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? HinhAnh { get; set; }

    public string? FullDomain { get; set; }
}
