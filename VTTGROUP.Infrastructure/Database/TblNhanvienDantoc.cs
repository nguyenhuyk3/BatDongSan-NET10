using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienDantoc
{
    public int Id { get; set; }

    public string MaDanToc { get; set; } = null!;

    public string? TenDanToc { get; set; }

    public string? GhiChu { get; set; }
}
