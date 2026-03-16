using VTTGROUP.Domain.Model.ChinhSachThanhToan;
using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.ChinhSachBanHang
{
    public class ChinhSachBanHangModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaDotMB { get; set; } = string.Empty;
        public string TenDotMB { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public string? NoiDung { get; set; }
        public List<ChinhSachBanHangChiTietModel>? ListChinhSachBHs { get; set; }
    }

    public class ChinhSachBanHangChiTietModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaCSBH { get; set; } = string.Empty;
        public string TenCSBH { get; set; } = string.Empty;
        public string MaHinhThucKM { get; set; } = string.Empty;
        public string TenHinhThucKM { get; set; } = string.Empty;
        public string MaLoaiDieuKienKM { get; set; } = string.Empty;
        public string TenLoaiDieuKienKM { get; set; } = string.Empty;
        public int SoLuongKM { get; set; } = 0;
        public string TuNgay { get; set; }
        public string DenNgay { get; set; }
        public decimal GiaTriKM { get; set; } = 0;
        public int SttUuTien { get; set; } = 0;
        /// <summary>
        /// Đánh dấu chính sách được load từ phiếu cũ (chỉ cho sửa STT ưu tiên)
        /// </summary>
        public bool IsOld { get; set; } = false;
    }
}
