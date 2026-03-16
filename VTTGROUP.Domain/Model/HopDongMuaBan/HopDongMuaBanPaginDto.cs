using System.Collections.Generic;
using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.HopDongMuaBan
{
    public class HopDongMuaBanPaginDto
    {
        public int STT { get; set; }
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaDatCoc { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaChinhSachThanhToan { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public DateTime NgayKy { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string IDLanDieuChinhKH { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int SoPhuLuc { get; set; }
        public bool IsDaKy { get; set; } = false;
        public DateTime NgayXacNhan { get; set; } = DateTime.Now;
        public string NguoiXacNhan { get; set; } = string.Empty;
        public string TenNguoiXacNhan { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
    public class HopDongMuaBanModel
    {
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaPhieuDC { get; set; } = string.Empty;
        public string SoPhieuDC { get; set; } = string.Empty;
        public string MaPhieuDK { get; set; } = string.Empty;
        public string? IDLanDieuChinhKH { get; set; }
        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? IDCard { get; set; }
        public DateTime? NgayCap { get; set; }
        public string? NoiCap { get; set; }
        public string? DiaChiThuongTru { get; set; }
        public string? DiaChiHienNay { get; set; }
        public DateTime NgayLap { get; set; }
        public string NgayKy { get; set; } = string.Empty;
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenCanHo { get; set; } = string.Empty;
        public string MaChinhSachTT { get; set; } = string.Empty;
        public string TenChinhSachTT { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal DonGiaDat { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal GiaCanHoTruocThue { get; set; } = 0;
        public decimal GiaCanHoSauThue { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public decimal TyLeCK { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public string GhiChu { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public string MaMauIn { get; set; } = string.Empty;
        public string TenMauIn { get; set; } = string.Empty;
        public List<KhachHangDongSoHuuHopDong>? ListKHDongSoHuu { get; set; }
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public bool IsDaKy { get; set; } = false;
        public DateTime NgayXacNhan { get; set; } = DateTime.Now;
        public string NguoiXacNhan { get; set; } = string.Empty;
        public string TenNguoiXacNhan { get; set; } = string.Empty;
        public string FlagDC { get; set; }
    }

    public class PhieuDatCocChuaLenHDModel
    {
        public string MaPhieuDC { get; set; } = string.Empty;
        public string SoPhieuDC { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaChinhSachTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public decimal GiaBan { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanSauCK { get; set; } = 0;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
    }

    public class HopDongMuaBanTienDoThanhToanModel
    {
        public string MaHopDong { get; set; } = string.Empty;
        public string MaPhuLuc { get; set; } = string.Empty;
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTT { get; set; } = 0;
        public string NoiDungTT { get; set; } = string.Empty;
        public string MaKyTT { get; set; } = string.Empty;
        public string TenKyTT { get; set; } = string.Empty;
        public int SoKhoangCachNgay { get; set; } = 0;
        public int DotThamChieu { get; set; }
        public decimal TyLeThanhToan { get; set; } = 0;
        public decimal TyLeThanhToanVAT { get; set; } = 0;
        public decimal SoTien { get; set; } = 0;
        public decimal SoTienCanTruDaTT { get; set; } = 0;
        public decimal SoTienPhaiThanhToan { get; set; } = 0;
    }

    public class KhachHangDongSoHuuHopDong
    {
        public string MaKhachHang { get; set; } = string.Empty;
        public string IDLanDieuChinh { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiLienLac { get; set; } = string.Empty;
        public bool ISKhachHangDaiDien { get; set; } = false;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string ChucVuNguoiDaiDien { get; set; } = string.Empty;
        public string SoDienThoaiDaiDien { get; set; } = string.Empty;
        public string NguoiLienHe { get; set; } = string.Empty;
        public string SoDienThoaiNguoiLienHe { get; set; } = string.Empty;
    }

}
