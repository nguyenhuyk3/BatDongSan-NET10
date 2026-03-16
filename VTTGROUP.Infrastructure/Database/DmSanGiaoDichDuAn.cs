using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DmSanGiaoDichDuAn
{
    public string MaSan { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public int? SoLuongBookCanHo { get; set; }
}
