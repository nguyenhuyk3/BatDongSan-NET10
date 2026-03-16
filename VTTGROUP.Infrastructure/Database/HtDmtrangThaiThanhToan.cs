using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDmtrangThaiThanhToan
{
    public int MaTrangThai { get; set; }

    public string? TenTrangThaiVi { get; set; }

    public string? TenTrangThaiEn { get; set; }

    public string? MaMau { get; set; }

    public int? SoNgay { get; set; }

    public string? GhiChu { get; set; }

    public string? MaMauNen { get; set; }

    public string? MaMauChu { get; set; }

    public string? MaMauSo { get; set; }
}
