using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienNguoiphuthuocMoiquanhe
{
    public int Id { get; set; }

    public string MaMoiQuanHe { get; set; } = null!;

    public string TenMoiQuanHe { get; set; } = null!;

    public string? GhiChu { get; set; }
}
