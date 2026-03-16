using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangChiTiet
{
    public int Id { get; set; }

    public string MaKhachHang { get; set; } = null!;

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public DateTime? NgaySinh { get; set; }

    public string? QuocTich { get; set; }

    public string? MaLoaiIdCard { get; set; }

    public string? IdCard { get; set; }

    public DateTime? NgayCapIdCard { get; set; }

    public string? NoiCapIdCard { get; set; }

    public string? DiaChiThuongTru { get; set; }

    public string? DiaChiLienLac { get; set; }

    public string? NguoiDaiDien { get; set; }

    public string? ChucVuNguoiDaiDien { get; set; }

    public string? NguoiLienHe { get; set; }

    public string? SoDienThoaiNguoiLienHe { get; set; }

    public string? SoDienThoaiDaiDien { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public string? IdlanDieuChinh { get; set; }

    public string? GioiTinh { get; set; }
}
