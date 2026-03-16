using VTTGROUP.Domain.Model.Block;
using VTTGROUP.Domain.Model.DMLoaiThietKe;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.LoaiGoc;
using VTTGROUP.Domain.Model.View;
using VTTGROUP.Domain.Model.ViewMatKhoi;
using VTTGROUP.Domain.Model.ViTri;

namespace VTTGROUP.Domain.Model.DuAn
{
    public class DuAnModel
    {
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? DiaChi { get; set; }
        public string? TinhThanh { get; set; }
        public string? XaPhuong { get; set; }
        public string? GhiChu { get; set; }
        public int? TrangThai { get; set; }
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public List<BlockModel> ListBlock { get; set; } = new List<BlockModel>();
        public List<LoaiCanHoModel> ListLoaiCan { get; set; } = new List<LoaiCanHoModel>();
        public List<LoaiDienTichModel> ListLoaiDT { get; set; } = new List<LoaiDienTichModel>();
        public List<LoaiThietKeModel> ListLoaiLayout { get; set; } = new List<LoaiThietKeModel>();
        public List<HuongModel> ListHuong { get; set; } = new List<HuongModel>();
        public List<ViewMatKhoiModel> ListVMK { get; set; } = new List<ViewMatKhoiModel>();
        public List<LoaiGocModel> ListLG { get; set; } = new List<LoaiGocModel>();
        public List<ViTriModel> ListVT { get; set; } = new List<ViTriModel>();
        public List<ViewModel> ListView { get; set; } = new List<ViewModel>();
    }

    public class LoaiDienTichModel
    {
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaLoaiDT { get; set; }
        public string? TenLoaiDT { get; set; }
        public decimal HeSo { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public int SoLuongCanHo { get; set; }
    }

    public class HuongModel
    {
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaHuong { get; set; }
        public string? TenHuong { get; set; }
        public decimal HeSo { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public int SoLuongCanHo { get; set; }
    }


    public class DanhMucDuAnPagingDto
    {
        public int STT { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public bool IsSelected { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
    }
    public class CauHinhDuAnModel
    {
        public int Id { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal? SoTienGiuCho { get; set; }
        public int? ThoiGianChoBookGioHangRieng { get; set; }
        public int? ThoiGianChoBookGioHangChung { get; set; }
        public decimal? SaiSoDoanhThuChoPhepKhbh { get; set; }
        public decimal? DonGiaTb { get; set; }
        public decimal? DonGiaDat { get; set; }
        public decimal? TyLeThueVat { get; set; }
        public decimal? TyLeQuyBaoTri { get; set; }
        public decimal? TyLeLaiQuaHan { get; set; }
        public int? NgayQuaHanTungDotChoPhep { get; set; }
        public string? PhuongThucTinhChietKhauKM { get; set; }
        public bool? IsKichHoatGh { get; set; }
        public bool? IsMoBanCoGia { get; set; }
        public int? SoLuongUserSanGd { get; set; }
        public bool? ChoPhepNhieuBookingCho1Can { get; set; }
        public int? SoLuongBookingToiDa { get; set; }
        public decimal? ChenhLechGiaTran { get; set; }
        public int? PhanSoLamTron { get; set; }
        public bool? IsHienThiGiaTran { get; set; }


    }

    public class ConfigFieldDto
    {
        public string? FieldName { get; set; }
        public string? DisplayName { get; set; }
        public string? Value { get; set; }
        public string? OriginalValue { get; set; }
        public bool IsEditing { get; set; } = false;
        public bool JsFormatted { get; set; } = false;
        public bool HasError { get; set; } = false;
        public bool IsCheckBox { get; set; } = false;
        public bool BoolValue
        {
            get => Convert.ToBoolean(Value);
            set => Value = value.ToString();
        }
    }
    public class LichSuConfigFieldDto
    {
        public string? TenTruong { get; set; }
        public string? GiaTri { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
    }
    public class SanGiaoDichBookingDto
    {
        public string MaSanGiaoDich { get; set; } = "";
        public string TenSanGiaoDich { get; set; } = "";
        public int SoLuongBooking { get; set; }
    }
}
