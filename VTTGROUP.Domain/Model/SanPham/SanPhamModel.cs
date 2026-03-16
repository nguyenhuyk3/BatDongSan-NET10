using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.SanPham
{
    public class SanPhamModel
    {
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;

        public DateTime NgayLap { get; set; }
        public NguoiLapModel NguoiLap { get; set; }
        public string? MaNhanVien { get; set; }
        public decimal HeSoCanHo { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaBlock { get; set; } = string.Empty;
        public string? TenBlock { get; set; } = string.Empty;
        public string MaTang { get; set; } = string.Empty;
        public string TenTang { get; set; } = string.Empty;
        public decimal? HeSoTang { get; set; }
        public string MaLoaiCan { get; set; } = string.Empty;
        public string TenLoaiCan { get; set; } = string.Empty;
        public string? MaLoaiDT { get; set; }
        public string? TenLoaiDT { get; set; }
        public decimal? HeSoDienTich { get; set; }
        public string? MaLoaiThietKe { get; set; }
        public string? TenLoaiThietKe { get; set; }
        public string MaLoaiGoc { get; set; } = string.Empty;
        public string TenLoaiGoc { get; set; } = string.Empty;
        public decimal? HeSoGoc { get; set; }
        public string MaViTri { get; set; } = string.Empty;
        public string TenViTri { get; set; } = string.Empty;
        public decimal? HeSoViTri { get; set; }
        public string MaView { get; set; } = string.Empty;
        public string TenView { get; set; } = string.Empty;
        public decimal? HeSoView { get; set; }
        public string MaTruc { get; set; } = string.Empty;
        public string TenTruc { get; set; } = string.Empty;
        public decimal? HeSoTruc { get; set; }
        public string MaMatKhoi { get; set; } = string.Empty;
        public string TenMatKhoi { get; set; } = string.Empty;
        public decimal HeSoMatKhoi { get; set; }
        public decimal DienTichTimTuong { get; set; }
        public decimal DienTichLotLong { get; set; }
        public decimal DienTichThongThuy { get; set; }
        public decimal DienTichSanVuon { get; set; }
        public string MaLoaiSP { get; set; } = string.Empty;
        public string TenLoaiSP { get; set; } = string.Empty;

    }

    public class SanPhamImportModel
    {
        public string? MaSanPham { get; set; }
        public string? TenSanPham { get; set; }
        public string? MaBlock { get; set; }
        public string? MaTang { get; set; }
        public string? MaTruc { get; set; }
        public string? MaLoaiCan { get; set; }
        public string? MaLoaiDienTich { get; set; }
        public string? MaLoaiLayout { get; set; }
        public decimal? DienTichTimTuong { get; set; }
        public decimal? DienTichThongThuy { get; set; }
        public decimal? DienTichSanVuon { get; set; }
        public decimal? HeSoCanHo { get; set; }
        public int ExcelRowIndex { get; set; }      // 
    }
}
