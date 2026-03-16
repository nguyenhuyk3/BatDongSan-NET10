namespace VTTGROUP.Domain.Model.InPhieu
{
    public class TemplatePhieuDatCoc
    {
        public string MaPhieu { get; set; }    
        public ThongTinDangKyNguyenVong ThongTin { get; set; }
    }
    public class ThongTinDangKyNguyenVong
    {
        public string MaMauIn { get; set; }
        public string MaPhieu { get; set; }
        public string TenCongTy { get; set; }
        public string TenKhachHang { get; set; }
        public string TenNguoiDaiDien { get; set; }
        public string NgaySinh { get; set; }
        public string QT { get; set; }
        public string SoCMND { get; set; }
        public string NgayCap { get; set; }
        public string NoiCap { get; set; }
        public string DiaChiThuongTru { get; set; }
        public string DiaChiHienTai { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string TenDuAn { get; set; }
        public string DiaChiCongTy { get; set; }
        public string MaCanHo { get; set; }
        public string TangBlock { get; set; }
        public string Tang { get; set; }
        public string Block { get; set; }
        public string DienTichThongThuy { get; set; }
        public string DienTichTimTuong { get; set; }
        public string GiaBanCanHo { get; set; }
        public string GiaBanCanHoBangChu { get; set; }
        public string PhiBaoTri { get; set; }
        public string NgayHienThai { get; set; }
        public string ThangHienTai { get; set; }
        public string NamHienTai { get; set; }

        public List<ChinhSachModel> DanhSachChinhSach { get; set; }
    }

    public class ChinhSachModel
    {
        public string NoiDung { get; set; }
        public bool DuocChon { get; set; } // true = ☑, false = ☐
    }
}
