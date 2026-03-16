using VTTGROUP.Domain.Model.Block;
using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.Tang;
using VTTGROUP.Domain.Model.View;
using VTTGROUP.Domain.Model.ViewMatKhoi;
using VTTGROUP.Domain.Model.ViewTruc;
using VTTGROUP.Domain.Model.ViTri;

namespace VTTGROUP.Domain.Model.CanHo
{
    public class CanHoModel
    {
        public string? MaCanHo { get; set; }
        public string? TenCanHo { get; set; }
        public DuAnModel? DuAn { get; set; }
        public BlockModel? Block { get; set; }
        public TangModel? Tang { get; set; }
        public LoaiCanHoModel? LoaiCanHo { get; set; }
        public ViTriModel? ViTri { get; set; }
        public ViewModel? View { get; set; }
        public ViewTrucModel? ViewTruc { get; set; }
        public ViewMatKhoiModel? ViewMatKhoi { get; set; }
        public string? LoaiView { get; set; }
        public string Status { get; set; } = "default";
    }

    public class SoDoCanHoModel
    {
        public string? MaSanPham { get; set; }
        public string? TenSanPham { get; set; }
        public string? MaTang { get; set; }
        public string? TenTang { get; set; }
        public decimal? HeSoTang { get; set; }
        public string? MaTruc { get; set; }
        public string? TenTruc { get; set; }
        public int? ThuTuHienThi { get; set; }
        public decimal? HeSoTruc { get; set; }
        public string? MaLoaiView { get; set; }
        public string? MatCanHo { get; set; }
        public string? MaView { get; set; }
        public string? TenView { get; set; } // "Mặt Trong" / "Mặt Ngoài"
        public string? MaHuong { get; set; }
        public string? TenHuong { get; set; }
        public decimal? HeSoMatCanHo { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaMauDot { get; set; }
        public bool? IsKhongChonLai { get; set; }
        public int? ThuTuTruc { get; set; }
        public float HeSoTuTinh { get; set; }
        public float DienTichTimTuong { get; set; }
        public float DienTichCanHo { get; set; }
        public float DienTichLotLong { get; set; }
        public float DienTichSanVuon { get; set; }
        public string? MaDotMoBan { get; set; }
        public string? DotMoBan { get; set; }
        public bool? IsMoBan { get; set; }
        public bool? IsDuocDangKy { get; set; }
        public string? TenViTri { get; set; }
        public string? TenMatKhoi { get; set; }
        public string? TenLoaiCanHo { get; set; }
        public string? MaLoaiThietKe { get; set; }
        public string? TenLoaiGoc { get; set; }
        public string? MaGioHang { get; set; }     
        public string? TinhTrangSanPham { get; set; }
        public string? MaMauTinhTrang { get; set; }
        public string? MaMauTrangThai { get; set; }
        public string? TenTrangThai { get; set; }
        public string? TrangThai { get; set; }
        public string? MaMauChu { get; set; }
        public string? FontWeghtMaCanHo { get; set; }
        public string? MaMauGoc { get; set; }
        public bool FlagPDK { get; set; }
        public int? ThoiGianHieuLuc { get; set; }
        public decimal? TongSoLuongCanTheoDot { get; set; }
        public decimal? TongSoLuongDaBan { get; set; }
        public decimal? DoanhThuDuKien { get; set; }
        public string? MaMau { get; set; }
        public bool? IsGoc { get; set; }
        public decimal? GiaBan { get; set; }// giá bán sau thuế
        public decimal? GiaBanChinhThuc { get; set; }// giá bán chính thwucs sau thuế
        public bool? IsMoBanGiaTran { get; set; }
        public bool? IsMoBanCoGia { get; set; }
        public string? PhuongThucTinhCk { get; set; }
        public decimal? GiaBanTruocThue { get; set; }
        public decimal? GiaBanChinhThucTruocThue { get; set; }        
    }
    public class TrucSoDoModel
    {
        public string MaTruc { get; set; }
        public string TenTruc { get; set; }
    }

    public class DangKyCountdownDto
    {
        public string MaCanHo { get; set; } = default!;
        public string MaPhieu { get; set; } = default!;
        public DateTime ExpireAtUtc { get; set; }      // thời điểm hết hạn
        public int RemainingSeconds { get; set; }      // server side (tham khảo)
    }

    public class HienTrangKinhDoanDto
    {
        public string MaTrangThai { get; set; } = null!;
        public string? TenTrangThai { get; set; }
        public string? MaMau { get; set; }
        public int? SoLuong { get; set; }
    }
}
