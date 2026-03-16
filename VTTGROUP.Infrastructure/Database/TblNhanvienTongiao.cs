using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienTongiao
{
    public int Id { get; set; }

    public string MaTonGia { get; set; } = null!;

    public string TenTonGia { get; set; } = null!;

    public string? GhiChu { get; set; }
}
