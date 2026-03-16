using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhKeHoachBanHangDotMoBanCanHo
{
    public int Id { get; set; }

    public string? MaPhieuKh { get; set; }

    public string? MaDotMoBan { get; set; }

    public string? MaCanHo { get; set; }

    public decimal? HeSoCanHo { get; set; }

    public decimal? DienTichCanHo { get; set; }
}
