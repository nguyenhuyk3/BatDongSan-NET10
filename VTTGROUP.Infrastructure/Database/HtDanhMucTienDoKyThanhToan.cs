using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDanhMucTienDoKyThanhToan
{
    public string? MaKyTt { get; set; }

    public string? TenKyTt { get; set; }

    public string? MaDuAn { get; set; }

    public DateTime? NgayDuKien { get; set; }

    public int? ThuTuHt { get; set; }
}
