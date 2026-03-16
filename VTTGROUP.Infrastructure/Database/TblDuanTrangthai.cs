using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanTrangthai
{
    public string MaTrangThai { get; set; } = null!;

    public string? TenTrangThai { get; set; }

    public string? MaMau { get; set; }
}
