using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiGoc
{
    public string MaDuAn { get; set; } = null!;

    public string MaLoaiGoc { get; set; } = null!;

    public string TenLoaiGoc { get; set; } = null!;

    public decimal? HeSoGoc { get; set; }

    public bool? IsGoc { get; set; }
}
