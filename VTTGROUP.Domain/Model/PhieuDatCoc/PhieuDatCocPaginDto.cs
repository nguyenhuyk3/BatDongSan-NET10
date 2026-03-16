using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuDatCoc
{
    public class PhieuDatCocPaginDto
    {
        public int STT { get; set; }
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaPhieuDangKy { get; set; } = string.Empty;
        public string SoPhieuDC { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaChinhSachTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaCanHoTruocThue { get; set; } = 0;
        public decimal GiaCanHoSauThue { get; set; } = 0;
        public decimal DonGiaDat { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal TyLeCK { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public decimal SoTienGiuCho { get; set; } = 0;

        public DateTime NgayKi { get; set; }
        public string GhiChu { get; set; } = string.Empty;
        public string IDKhachHangCT { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public bool IsDaKy { get; set; } = false;
        public DateTime NgayXacNhan { get; set; } = DateTime.Now;
        public string NguoiXacNhan { get; set; } = string.Empty;
        public string TenNguoiXacNhan { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaPhieuThanhLy { get; set; } = string.Empty;
        public string SoPhieuThanhLy { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
    public class PhieuDatCocModel
    {
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaDatCocChoKyLai { get; set; } = string.Empty;
        public string SoPhieuDC { get; set; } = string.Empty;
        public string MaPhieuDK { get; set; } = string.Empty;
        public string? IDKhachHangCT { get; set; }
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
        public string NgayKi { get; set; } = string.Empty;
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
        public string GhiChu { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public string MaMauIn { get; set; } = string.Empty;
        public string TenMauIn { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaCanHoTruocThue { get; set; } = 0;
        public decimal GiaCanHoSauThue { get; set; } = 0;
        public decimal DonGiaDat { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal TyLeCK { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public bool IsDaKy { get; set; } = false;
        public DateTime NgayXacNhan { get; set; } = DateTime.Now;
        public string NguoiXacNhan { get; set; } = string.Empty;
        public string TenNguoiXacNhan { get; set; } = string.Empty;
        public string FlagDK { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string ChucVuNguoiDaiDien { get; set; } = string.Empty;
        public string NguoiLienHe { get; set; } = string.Empty;
        public string SoDienThoaiNguoiLienHe { get; set; } = string.Empty;
        public string MaDoiTuongKH { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class PhieuDangKyDatCocModel
    {
        public string MaPhieuDangKy { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenCanHo { get; set; } = string.Empty;
        public string MaDatCocChoKyLai { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCSTT { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
    }

    public class PhieuDangKyDatCocCSTTModel
    {
        public string MaCSTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
    }
    public class PhieuDatCocTienDoThanhToanModel
    {
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTT { get; set; } = 0;
        public string NoiDungTT { get; set; } = string.Empty;
        public string MaKyTT { get; set; } = string.Empty;
        public string TenKyTT { get; set; } = string.Empty;
        public DateTime? NgayThanhToan { get; set; } = null;
        public int SoKhoangCachNgay { get; set; } = 0;
        public int DotThamChieu { get; set; }
        public decimal TyLeThanhToan { get; set; } = 0;
        public decimal TyLeThanhToanVAT { get; set; } = 0;
        public decimal SoTien { get; set; } = 0;
        public decimal SoTienCanTruDaTT { get; set; } = 0;
        public decimal SoTienChuyenDoiBooking { get; set; } = 0;
        public decimal SoTienPhaiThanhToan { get; set; } = 0;
    }

    public class PhieuDatCocInDto
    {
        public string MaPhieu { get; set; }
        public string TenKhachHang { get; set; }
        public string NgayLap { get; set; }
        public string GhiChu { get; set; }
        public string NgaySinh { get; set; }
        public string QT { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string IdCard { get; set; }
        public DateTime NgayCapIdCard { get; set; }
        public string NoiCapIdCard { get; set; }
        public string DiaChiThuongTru { get; set; }
        public string DiaChiLienLac { get; set; }

        public string TenDuAn { get; set; }
        public string MaCanHo { get; set; }
        public string TenSanPham { get; set; }

        public string Tang { get; set; }
        public string MaTang { get; set; }
        public string MaBlock { get; set; }
        public string Block { get; set; }
        public decimal GiaBanCanHo { get; set; }
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaCanHoTruocThue { get; set; } = 0;
        public decimal GiaCanHoSauThue { get; set; } = 0;
        public decimal DonGiaDat { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal TyLeCK { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public string TenCongTy { get; set; }
        public string DiaChiCongTy { get; set; }
        public string TenNguoiDaiDien { get; set; }
        public string CmndNgayCapNguoiDDCT { get; set; }
        public string CmndNoiCapNguoiDDCT { get; set; }
        public string CmndSoNguoiDaiDienCT { get; set; }
        public string MaMauIn { get; set; }
    }

    public class ExportProgress
    {
        public int Percent { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Index { get; set; }
        public int Total { get; set; }
        public string? FileName { get; set; }

        public ExportProgress() { }

        public ExportProgress(int percent, string message, int index, int total, string? fileName = null)
        {
            Percent = percent; Message = message; Index = index; Total = total; FileName = fileName;
        }
    }
    public class CongNoPhaiThuModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string DuAn { get; set; } = string.Empty;
        public string MaChungTu { get; set; } = string.Empty;
        public string IdChungTu { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public DateTime HanThanhToan { get; set; }
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public decimal SoTien { get; set; }
        public string MaCongViec { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = string.Empty;
    }
}
