using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienNganhang
{
    public string MaNhanVien { get; set; } = null!;

    public string SoTaiKhoanNh { get; set; } = null!;

    public string? TenTaiKhoanNh { get; set; }

    public string? MaNganHang { get; set; }

    public string? MaChiNhanh { get; set; }

    public bool? TrangThaiChiLuong { get; set; }

    public string? DiaChiNganHang { get; set; }
}
