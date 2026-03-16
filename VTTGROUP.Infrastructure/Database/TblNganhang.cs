using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNganhang
{
    public int Id { get; set; }

    public string MaNganHang { get; set; } = null!;

    public string TenNganHang { get; set; } = null!;

    public string? GhiChu { get; set; }
}
