using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucViTri
{
    public string MaDuAn { get; set; } = null!;

    public string MaViTri { get; set; } = null!;

    public string TenViTri { get; set; } = null!;

    public decimal? HeSoViTri { get; set; }
}
