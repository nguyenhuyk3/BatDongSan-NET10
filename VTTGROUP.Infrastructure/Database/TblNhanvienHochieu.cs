using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienHochieu
{
    public int Id { get; set; }

    public string MaLoaiHoChieu { get; set; } = null!;

    public string TenLoaiHoChieu { get; set; } = null!;

    public string? GhiChu { get; set; }
}
