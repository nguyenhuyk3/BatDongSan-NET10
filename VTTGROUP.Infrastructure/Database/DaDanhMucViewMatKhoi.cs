using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucViewMatKhoi
{
    public string MaDuAn { get; set; } = null!;

    public string MaMatKhoi { get; set; } = null!;

    public string TenMatKhoi { get; set; } = null!;

    public decimal? HeSoMatKhoi { get; set; }
}
