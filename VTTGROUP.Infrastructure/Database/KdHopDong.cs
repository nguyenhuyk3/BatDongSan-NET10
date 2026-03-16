using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdHopDong
{
    public string MaHopDong { get; set; } = null!;

    public string? SoHopDong { get; set; }

    public DateTime? NgayKy { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaDatCoc { get; set; }

    public string? MaChinhSachThanhToan { get; set; }

    public string? MaCanHo { get; set; }

    public decimal? GiaDat { get; set; }

    public decimal? TyLeThueVat { get; set; }

    public decimal? GiaBanTruocThue { get; set; }

    public decimal? GiaBanSauThue { get; set; }

    public string? NoiDung { get; set; }

    public string? MaKhachHang { get; set; }

    public string? IdlanDieuChinhKh { get; set; }

    public decimal? DienTichTimTuong { get; set; }

    public decimal? DienTichLotLong { get; set; }

    public decimal? DienTichSanVuon { get; set; }

    public decimal? GiaBanTienThue { get; set; }

    public decimal? TyLeQuyBaoTri { get; set; }

    public decimal? TienQuyBaoTri { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? MaMauIn { get; set; }

    public bool? IsDaKy { get; set; }

    public DateTime? NgayXacNhan { get; set; }

    public string? NguoiXacNhan { get; set; }

    public decimal? GiaCanHoTruocThue { get; set; }

    public decimal? GiaCanHoSauThue { get; set; }

    public decimal? DonGiaDat { get; set; }

    public decimal? TyLeCk { get; set; }

    public decimal? GiaTriCk { get; set; }
}
