using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienPhongban
{
    public int Id { get; set; }

    public string MaPhongBan { get; set; } = null!;

    public string? TenPhongBan { get; set; }

    public string? GhiChu { get; set; }
}
