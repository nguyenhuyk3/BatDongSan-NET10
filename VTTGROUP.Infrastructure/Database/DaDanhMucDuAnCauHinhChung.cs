using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucDuAnCauHinhChung
{
    public int Id { get; set; }

    public string? MaDuAn { get; set; }

    public decimal? SoTienGiuCho { get; set; }

    /// <summary>
    /// Số phút
    /// </summary>
    public int? ThoiGianChoBookGioHangRieng { get; set; }

    /// <summary>
    /// Số phút
    /// </summary>
    public int? ThoiGianChoBookGioHangChung { get; set; }

    public decimal? SaiSoDoanhThuChoPhepKhbh { get; set; }

    public decimal? DonGiaTb { get; set; }

    public decimal? DonGiaDat { get; set; }

    public decimal? TyLeThueVat { get; set; }

    public decimal? TyLeQuyBaoTri { get; set; }

    public bool? IsKichHoatGh { get; set; }

    public bool? IsMoBanCoGia { get; set; }

    public int? SoLuongUserSanGd { get; set; }

    public decimal? TyLeLaiQuaHan { get; set; }

    public int? NgayQuaHanTungDotChoPhep { get; set; }

    public bool? ChoPhepNhieuBookingCho1Can { get; set; }

    public int? SoLuongBookingToiDa { get; set; }

    public decimal? ChenhLechGiaTran { get; set; }

    public int? PhanSoLamTron { get; set; }

    public bool? IsHienThiGiaTran { get; set; }

    /// <summary>
    /// 1: Chiết khấu thẳng; 2: Chiết khấu theo thứ tự
    /// </summary>
    public string? PhuongThucTinhChietKhauKm { get; set; }
}
