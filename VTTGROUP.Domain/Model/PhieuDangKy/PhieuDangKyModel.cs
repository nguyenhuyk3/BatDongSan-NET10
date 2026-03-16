using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuDangKy
{
    public class PhieuDangKyModel
    {
        public string MaPhieu { get; set; } = null!;
        public string MaDuAn { get; set; } = null!;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = null!;
        public string TenCanHo { get; set; } = null!;
        public string? MaChinhSachTt { get; set; }
        public string? TenChinhSachTt { get; set; }
        public string? SanGiaoDich { get; set; }
        public string? TenSanGiaoDich { get; set; }
        public string? NhanVienMoiGioi { get; set; }
        public string? TenNhanVienMoiGioi { get; set; }
        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public DateTime? NgayLap { get; set; }
        public string? DotMoBan { get; set; }
        public string TenDotMoBan { get; set; } = string.Empty;
        public string? TrangThai { get; set; }
        public string? IdkhachHangCt { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string MaGioHang { get; set; } = string.Empty;
        public bool LoaiGioHang { get; set; } = false;
        public string? MaPhieuBooking { get; set; } = string.Empty;
        public bool IsXacNhan { get; set; } = false;
        public bool IsHetHieuLuc { get; set; } = false;
        public string? NoiDung { get; set; } = string.Empty;
        public string? MaLoaiThietKe { get; set; } = string.Empty;
       // public bool ISMoBanCoGia { get; set; } = false;
        public bool IsXacNhanChuyenCoc { get; set; } = false;
        public decimal TyLeChietKhau { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public decimal? GiaBanTheoCSTT { get; set; }
        public decimal? GiaBan { get; set; }// giá bán sau thuế
       
        public bool? IsMoBanGiaTran { get; set; }
        public bool? IsMoBanCoGia { get; set; }
        public string? PhuongThucTinhCk { get; set; }
        public decimal? GiaBanChinhThuc { get; set; }// giá bán chính thức sau thuế
        public decimal? GiaBanTruocThue { get; set; }
        public decimal? GiaBanChinhThucTruocThue { get; set; }
        public decimal? DienTichCanHo { get; set; }// Diện tích xây dựng
        public decimal? DienTichLotLong { get; set; }// Diện tích sử dụng
        public decimal? DienTichSanVuon { get; set; }// Diện tích sân vườn
        public List<PhieuDangKyChinhSachBanHang> ListPDKCSBH = new List<PhieuDangKyChinhSachBanHang>();
        public string MaPhieuDC { get; set; } = string.Empty;
    }

    public class PhieuDangKyChinhSachBanHang
    {
        public string LoaiCS { get; set; } = string.Empty;
        public string TenLoaiCS { get; set; } = string.Empty;
        public string MaCS { get; set; } = string.Empty;
        public string TenCS { get; set; } = string.Empty;
        public string HinhThucKM { get; set; } = string.Empty;
        public decimal? GiaTriKM { get; set; }// Thành tiền/Tỷ lệ
        public decimal? ThanhTienKMGiaBan { get; set; }// Giá trị ưu đãi theo chính sách có giá trần
        public decimal? ThanhTienKMGiaBanChinhThuc { get; set; }// Giá trị ưu đãi theo chính sách chính thức được lưu ngầm dưới DB không có giá trần
        public decimal? ThanhTienKMGiaBanDuocChon { get; set; }// Giá trị ưu đãi chọn có giá trần
        public decimal? ThanhTienKMGiaBanChinhThucDuocChon { get; set; }// Thành tiền khuyến mãi giá bán chính thức được chọn lưu ngầm dưới DB không có giá trần
        public bool IsChon { get; set; } = false;
        public bool IsCKTyLe { get; set; } = false;
        public bool IsCheckBox { get; set; } = false;//=true thì là checkbox còn ngược lại là radio button
        public int SttUuTien { get; set; }
    }

    public class PhieuDangKyPagingDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public decimal GiaBan { get; set; } = 0;
        public decimal GiaBanTheoCSTT { get; set; } = 0;
        public string MaSanMoiGioi { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string TenNhanVienMG { get; set; } = string.Empty;
        public bool TrangThai { get; set; } = false;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public string TenCanHo { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string MaGioHang { get; set; } = string.Empty;
        public bool LoaiGioHang { get; set; } = false;
        public string MaPhieuBooking { get; set; } = string.Empty;
        public bool IsXacNhan { get; set; } = false;
        public bool IsHetHieuLuc { get; set; } = false;
        public string NoiDung { get; set; } = string.Empty;
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaPhieuThanhLy { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
        public bool IsXacNhanChuyenCoc { get; set; } = false;
        public decimal? GiaBanSauThue { get; set; }// giá bán chính thức sau thuế
        public decimal? GiaBanTruocThue { get; set; }
        public decimal? GiaBanChinhThucTruocThue { get; set; }
    }
}
