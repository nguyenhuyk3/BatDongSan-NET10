using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtQuyTrinhDuyet
{
    public int Id { get; set; }

    public string? TenQuyTrinh { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? MaCongViec { get; set; }

    public int? TrangThai { get; set; }

    public string? GhiChu { get; set; }
}
