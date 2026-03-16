using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienNganhnghe
{
    public int Id { get; set; }

    public string MaNganhNghe { get; set; } = null!;

    public string TenNganhNghe { get; set; } = null!;
}
