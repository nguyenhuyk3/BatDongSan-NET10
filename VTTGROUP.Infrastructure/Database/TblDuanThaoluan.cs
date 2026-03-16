using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanThaoluan
{
    public int Id { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaNhanVien { get; set; }

    public string? NoiDung { get; set; }

    public DateTime? NgayLap { get; set; }
}
