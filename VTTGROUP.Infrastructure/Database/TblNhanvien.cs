using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhanvien
{
    public int Id { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string HoVaTen { get; set; } = null!;

    public string? MaChamCong { get; set; }

    public DateTime? NgaySinh { get; set; }

    /// <summary>
    /// 0 = Nam
    /// 
    /// 1 = Nữ
    /// 
    /// 2 = Khác
    /// </summary>
    public byte GioiTinh { get; set; }

    public string? NoiSinh { get; set; }

    public string? NguyenQuan { get; set; }

    public string? MaQuocTich { get; set; }

    /// <summary>
    /// 0 = Độc thân 
    /// 
    /// 1 = Kết hôn 3 = Ly hôn
    /// </summary>
    public byte? TinhTrangHonNhan { get; set; }

    public string? MaDanToc { get; set; }

    public string? MaTonGia { get; set; }

    public string? MaSoThueCaNhan { get; set; }

    public string? MaTrinhDoPhoThong { get; set; }

    public int? SoNamKinhNghiem { get; set; }

    public string? MaNganhNghe { get; set; }

    public string? MaTrinhDoHocVan { get; set; }

    public string? MaCanCuoc { get; set; }

    public DateTime? NgayCapCc { get; set; }

    public string? UrlCccdmatSau { get; set; }

    public string? UrlCccdmatTruoc { get; set; }

    public string? NoiCapCc { get; set; }

    public string? MaLoaiHoChieu { get; set; }

    public string? SoHoChieu { get; set; }

    public DateTime? NgayCapHoChieu { get; set; }

    public DateTime? NgayHetHanHoChieu { get; set; }

    public string? NoiCapHoChieu { get; set; }

    public string? UrlHoChieu { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? DiaChiThuongTru { get; set; }

    public string? MaThuongTru { get; set; }

    public string? DiaChiTamTru { get; set; }

    public string? MaDiaChiTamTru { get; set; }

    public string? GhiChu { get; set; }

    public string? UrlDaiDien { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaPhongBan { get; set; }

    public string? MaChucVu { get; set; }

    public byte? TrangThai { get; set; }

    public string? SoDienThoai2 { get; set; }

    public string? EmailCongTy { get; set; }

    public string? MaSanGiaoDich { get; set; }

    public string? MaDuAn { get; set; }
}
