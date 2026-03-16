using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtHistoryLog
{
    public int Id { get; set; }

    public string? MaDuAn { get; set; }

    public string? TenBang { get; set; }

    public string? TenTruong { get; set; }

    public string? GiaTri { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? NguoiCapNhat { get; set; }
}
