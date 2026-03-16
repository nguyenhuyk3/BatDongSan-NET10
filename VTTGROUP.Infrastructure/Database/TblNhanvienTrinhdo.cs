using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienTrinhdo
{
    public int Id { get; set; }

    public string MaTrinhDo { get; set; } = null!;

    public string TenTrinhDo { get; set; } = null!;

    public string? GhiChu { get; set; }
}
