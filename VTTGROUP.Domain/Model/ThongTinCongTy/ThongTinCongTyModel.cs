namespace VTTGROUP.Domain.Model.ThongTinCongTy
{
    public class ThongTinCongTyModel
    {
        public string MaCongTy { get; set; } = null!;

        public string? TenCongTy { get; set; }

        public string? DiaChiCongTy { get; set; }

        public string? Fax { get; set; }

        public string? DienThoai { get; set; }

        public string? MaSoThue { get; set; }

        public string? TaiKhoan { get; set; }

        public string? ChiNhanhNganHang { get; set; }

        public string? DaiDienCongTy { get; set; }

        public string? ChucVuNguoiDaiDien { get; set; }

        public string? CmndSoNguoiDaiDien { get; set; }

        public DateTime? CmndNgayCapNguoiDd { get; set; }

        public string? CmndNoiCapNguoiDd { get; set; }

        public string? Email { get; set; }

        public string? TenTaiKhoan { get; set; }

        public string? TenNganHang { get; set; }

        public string? TenChiNhanh { get; set; }

        public List<UploadedFileModel> Logo { get; set; } = new List<UploadedFileModel>();
    }
}
