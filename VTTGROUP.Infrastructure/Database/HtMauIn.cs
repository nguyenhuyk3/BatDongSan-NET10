using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtMauIn
{
    public string MaMauIn { get; set; } = null!;

    public string? TenMauIn { get; set; }

    public string? MaDuAn { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? LoaiMauIn { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? NoiDung { get; set; }
}
