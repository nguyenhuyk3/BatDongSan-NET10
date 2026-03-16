using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanTiendoduan
{
    public int Id { get; set; }

    public string MaTienDo { get; set; } = null!;

    public string TenTienDo { get; set; } = null!;

    public string NoiDung { get; set; } = null!;
}
