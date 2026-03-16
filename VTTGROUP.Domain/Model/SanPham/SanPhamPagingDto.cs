namespace VTTGROUP.Domain.Model.SanPham
{
    public class SanPhamPagingDto
    {
        public int STT { get; set; }
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaBlock { get; set; } = string.Empty;
        public string TenBlock { get; set; } = string.Empty;
        public string MaTang { get; set; } = string.Empty;
        public string TenTang { get; set; } = string.Empty;
        public decimal HeSoTang { get; set; } = 1;
        public string MaTruc { get; set; } = string.Empty;
        public string TenTruc { get; set; } = string.Empty;
        public decimal HeSoTruc { get; set; } = 1;
        public string MaLoaiCan { get; set; } = string.Empty;
        public string TenLoaiCanHo { get; set; } = string.Empty;
        public string MaLoaiDienTich { get; set; } = string.Empty;
        public string TenLoaiDT { get; set; } = string.Empty;
        public decimal HeSoDT { get; set; } = 1;
        public string MaLoaiLayout { get; set; } = string.Empty;
        public string TenLoaiThietKe { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichThongThuy { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public string MaLoaiGoc { get; set; } = string.Empty;
        public string TenLoaiGoc { get; set; } = string.Empty;
        public decimal HeSoGoc { get; set; } = 1;
        public string MaLoaiView { get; set; } = string.Empty;
        public string TenView { get; set; } = string.Empty;
        public decimal HeSoView { get; set; } = 1;
        public string MaViewMatKhoiMaKhoi { get; set; } = string.Empty;
        public string TenMatKhoi { get; set; } = string.Empty;
        public decimal HeSoMatKhoi { get; set; } = 1;
        public string MaViTri { get; set; } = string.Empty;
        public string TenViTri { get; set; } = string.Empty;
        public decimal HeSoViTri { get; set; } = 1;
        public decimal HeSoCanHo { get; set; } = 1;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
        public int SoLanTrongKeHoach { get; set; }
        public bool KhongDuocXoa { get; set; }
        public string LoaiXuLy { get; set; } = string.Empty;
    }
    public class TemplateSanPhamTabDuAnDto
    {
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaBlock { get; set; } = string.Empty;
        public string TenBlock { get; set; } = string.Empty;
        public string MaTang { get; set; } = string.Empty;
        public string TenTang { get; set; } = string.Empty;
        public decimal HeSoTang { get; set; } = 1;
        public string MaLoaiCanHo { get; set; } = string.Empty;
        public string TenLoaiCanHo { get; set; } = string.Empty;
        public string MaTruc { get; set; } = string.Empty;
        public string TenTruc { get; set; } = string.Empty;
        public string MaLoaiDT { get; set; } = string.Empty;
        public string TenLoaiDT { get; set; } = string.Empty;
        public string MaLoaiThietKe { get; set; } = string.Empty;
        public string TenLoaiThietKe { get; set; } = string.Empty;
    }
}
