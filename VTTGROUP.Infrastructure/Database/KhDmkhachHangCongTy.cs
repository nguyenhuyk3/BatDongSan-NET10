using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangCongTy
{
    public int Id { get; set; }

    public string MaKhachHang { get; set; } = null!;

    public string MaSoThue { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string SoDt { get; set; } = null!;

    public string? Email { get; set; }

    public string? TenNguoiDaiDien { get; set; }

    public string? SoCccd { get; set; }

    public DateTime? NgayCapCccd { get; set; }

    public string? NoiCapCccd { get; set; }

    public string? SoHoChieu { get; set; }

    public DateTime? NgayCapHoChieu { get; set; }

    public string? NoiCapHoChieu { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? IdlanDieuChinh { get; set; }

    public virtual KhDmkhachHang MaKhachHangNavigation { get; set; } = null!;
}
