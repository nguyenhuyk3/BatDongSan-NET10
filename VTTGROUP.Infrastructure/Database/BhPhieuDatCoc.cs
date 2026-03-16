using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDatCoc
{
    public string MaPhieuDc { get; set; } = null!;

    public string? SoPhieuDc { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaPhieuDangKy { get; set; }

    public string? MaDotMoBan { get; set; }

    public string? MaKhachHang { get; set; }

    public string? MaCanHo { get; set; }

    public string? MaChinhSachTt { get; set; }

    public decimal? DienTichTimTuong { get; set; }

    public decimal? DienTichLotLong { get; set; }

    public decimal? DienTichSanVuon { get; set; }

    public decimal? TyLeThueVat { get; set; }

    public decimal? GiaCanHoTruocThue { get; set; }

    public decimal? GiaCanHoSauThue { get; set; }

    public decimal? DonGiaDat { get; set; }

    public decimal? GiaDat { get; set; }

    public decimal? TyLeCk { get; set; }

    public decimal? GiaTriCk { get; set; }

    public decimal? GiaBanTruocThue { get; set; }

    public decimal? GiaBanSauThue { get; set; }

    public decimal? GiaBanTienThue { get; set; }

    public decimal? TyLeQuyBaoTri { get; set; }

    public decimal? TienQuyBaoTri { get; set; }

    public DateTime? NgayKi { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public string? IdkhachHangCt { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? MaPhieuDatCocKyLai { get; set; }

    public string? MaMauIn { get; set; }

    public bool? IsDaKy { get; set; }

    public DateTime? NgayXacNhan { get; set; }

    public string? NguoiXacNhan { get; set; }
}
