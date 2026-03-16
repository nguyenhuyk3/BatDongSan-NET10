using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtHienTrangKinhDoanh
{
    public string MaTrangThai { get; set; } = null!;

    public string? TenTrangThai { get; set; }

    public string? MaMau { get; set; }

    public string? MaMauChu { get; set; }

    public string? MaMauGoc { get; set; }

    public string? FontWeghtMaCanHo { get; set; }
}
