using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangTam
{
    public string MaKhachHangTam { get; set; } = null!;

    public string TenKhachHang { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public string? MaNhanVien { get; set; }

    /// <summary>
    /// C: cá nhân,T: tổ chức
    /// </summary>
    public string? MaDoiTuongKhachHang { get; set; }

    public string? SoDienThoai { get; set; }

    public string? Email { get; set; }

    public string? QuocTich { get; set; }

    public DateTime? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? MaLoaiIdCard { get; set; }

    public string? IdCard { get; set; }

    public DateTime? NgayCapIdCard { get; set; }

    public string? NoiCapIdCard { get; set; }

    public string? DiaChiThuongTru { get; set; }

    public string? DiaChiHienNay { get; set; }

    public string? MaNguonKhach { get; set; }

    public string MaDuAn { get; set; } = null!;

    public string? MaSanGd { get; set; }

    public string? NguoiDaiDien { get; set; }

    public string? SoDienThoaiNguoiDaiDien { get; set; }

    public string? NguoiLienHe { get; set; }

    public string? ChucVuNguoiDaiDien { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<KhDmkhachHangNguon> KhDmkhachHangNguons { get; set; } = new List<KhDmkhachHangNguon>();

    public virtual KhDmloaiCard? MaLoaiIdCardNavigation { get; set; }

    public virtual DmSanGiaoDich? MaSanGdNavigation { get; set; }

    public virtual HtDmquocGium? QuocTichNavigation { get; set; }
}
