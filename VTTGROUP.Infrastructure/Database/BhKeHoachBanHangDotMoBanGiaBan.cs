using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhKeHoachBanHangDotMoBanGiaBan
{
    public string MaPhieuKh { get; set; } = null!;

    public string MaDotMoBan { get; set; } = null!;

    public decimal DonGiaTbdot { get; set; }

    public bool? IsXacNhan { get; set; }
}
