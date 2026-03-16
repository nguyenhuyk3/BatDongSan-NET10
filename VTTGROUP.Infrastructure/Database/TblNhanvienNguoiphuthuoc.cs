using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvienNguoiphuthuoc
{
    public int Id { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string HoVaTen { get; set; } = null!;

    public string MaMoiQuanHe { get; set; } = null!;

    public DateTime NgaySinh { get; set; }

    public string? SoDienThoai { get; set; }

    public string? SoCmtcc { get; set; }

    public DateTime? NgayCap { get; set; }

    public string? NoiCap { get; set; }

    public bool? PhuThuoc { get; set; }

    public double? SoTienGiamTru { get; set; }

    public DateTime? NgayBatDauGiamTru { get; set; }

    public DateTime? NgayKetThucGiamTru { get; set; }

    public string? GhiChu { get; set; }
}
