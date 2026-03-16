using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.HopDongChuyenNhuong
{
    public class HopDongChuyenNhuongModel
    {
        public string MaPhieu { get; set; } = null!;      
        public string MaHopDong { get; set; } = null!;
        public string MaKhachHang { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string MaCanHo { get; set; } = null!;
        public string TenCanHo { get; set; } = null!;
        public string MaDuAn { get; set; } = null!;
        public string TenDuAn { get; set; } = null!;
        public string NgayChuyenNhuong { get; set; }
        public DateTime NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }      
        public string MaNhanVien { get; set; }
        public string TrangThai { get; set; } = null!;
        public string? GhiChu { get; set; }
        public decimal? GiaTriCanHo { get; set; } = null!;
        public decimal GiaTriDaThanhToan { get; set; }      
        public decimal PhiBaoTri { get; set; }
        public decimal PhiBaoTriDaThanhToan { get; set; }       
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
    }
}
