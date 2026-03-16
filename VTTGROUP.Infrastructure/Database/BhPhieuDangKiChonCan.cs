using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDangKiChonCan
{
    public string MaPhieu { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string? MaCanHo { get; set; }

    public string? MaChinhSachTt { get; set; }

    public string? SanGiaoDich { get; set; }

    public string? NhanVienMoiGioi { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaBanTheoCstt { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? DotMoBan { get; set; }

    public string? MaGioHang { get; set; }

    public bool? LoaiGioHang { get; set; }

    public string? MaKhachHangTam { get; set; }

    public string? MaPhieuBooking { get; set; }

    public bool? IsXacNhan { get; set; }

    public bool? IsHetHieuLuc { get; set; }

    public string? NoiDung { get; set; }

    public bool? IsMoBanCoGia { get; set; }

    public decimal? GiaBanChinhThuc { get; set; }

    public decimal? GiaBanTruocThue { get; set; }

    public decimal? GiaBanChinhThucTruocThue { get; set; }

    public string? PhuongThucTinhChietKhauKm { get; set; }

    public decimal? DienTichTimTuong { get; set; }

    public decimal? DienTichThongThuy { get; set; }

    public decimal? DienTichSanVuon { get; set; }

    public bool? IsMoBanGiaTran { get; set; }

    public bool? IsXacNhanChuyenCoc { get; set; }
}
