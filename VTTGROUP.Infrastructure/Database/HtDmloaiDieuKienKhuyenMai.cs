using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDmloaiDieuKienKhuyenMai
{
    public string MaLoaiDieuKienKm { get; set; } = null!;

    public string? TenLoaiDieuKienKm { get; set; }

    public bool? IsLoaiBooking { get; set; }
}
