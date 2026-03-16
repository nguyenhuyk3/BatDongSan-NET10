using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienChucvu
{
    public int Id { get; set; }

    public string MaChucVu { get; set; } = null!;

    public string? TenChucVu { get; set; }

    public string? GhiChu { get; set; }
}
