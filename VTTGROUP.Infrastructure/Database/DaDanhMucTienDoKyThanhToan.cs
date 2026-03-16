using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucTienDoKyThanhToan
{
    public string MaKyTt { get; set; } = null!;

    public string? TenKyTt { get; set; }

    public string? MaDuAn { get; set; }

    public DateTime? NgayDuKien { get; set; }

    public int? ThuTuHt { get; set; }
}
