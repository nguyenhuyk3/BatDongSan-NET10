using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhGioHangLichSuLgh
{
    public int Id { get; set; }

    public string? MaPhieu { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? MaNhanVien { get; set; }

    public bool? LoaiGioHangCu { get; set; }

    public bool? LoaiGioHangMoi { get; set; }

    public string? MaSanGiaoDichCu { get; set; }

    public string? MaSanGiaoDichMoi { get; set; }
}
