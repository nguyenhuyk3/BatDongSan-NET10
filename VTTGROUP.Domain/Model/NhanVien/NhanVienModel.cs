using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Domain.Model.DuAn;

namespace VTTGROUP.Domain.Model.NhanVien
{
    public class NhanVienModel
    {
        public int? Id { get; set; } = null!;
        public string MaNhanVien { get; set; } = null!;

        public string HoVaTen { get; set; } = null!;
        public string? MaChamCong { get; set; }

        public string? NgaySinh { get; set; } = null!;

        /// <summary>
        /// 0 = Nam
        /// 
        /// 1 = Nữ
        /// 
        /// 2 = Khác
        /// </summary>
        public byte GioiTinh { get; set; }

        public string? NoiSinh { get; set; }

        public string? NguyenQuan { get; set; }

        public string MaQuocTich { get; set; } = null!;

        /// <summary>
        /// 0 = Độc thân 
        /// 
        /// 1 = Kết hôn 3 = Ly hôn
        /// </summary>
        public byte? TinhTrangHonNhan { get; set; }

        public string MaDanToc { get; set; } = null!;

        public string? MaTonGia { get; set; }

        public string? MaSoThueCaNhan { get; set; }

        public string? MaTrinhDoPhoThong { get; set; }

        public int? SoNamKinhNghiem { get; set; }

        public string? MaNganhNghe { get; set; }

        public string MaTrinhDoHocVan { get; set; } = null!;

        public string MaCanCuoc { get; set; } = null!;

        public string? NgayCapCc { get; set; }

        public string? UrlCccdmatSau { get; set; }

        public string? UrlCccdmatTruoc { get; set; }

        public string NoiCapCc { get; set; } = null!;

        public string? MaLoaiHoChieu { get; set; }

        public string? SoHoChieu { get; set; }

        public DateTime? NgayCapHoChieu { get; set; }

        public DateTime? NgayHetHanHoChieu { get; set; }

        public string? NoiCapHoChieu { get; set; }

        public string? UrlHoChieu { get; set; }

        public string SoDienThoai { get; set; } = null!;
        public string? SoDienThoai2 { get; set; }

        public string Email { get; set; } = null!;
        public string? EmailCongTy { get; set; }

        public string? DiaChiThuongTru { get; set; }

        public string? MaThuongTru { get; set; }

        public string? DiaChiTamTru { get; set; }

        public string? MaDiaChiTamTru { get; set; }

        public string? GhiChu { get; set; }

        public string? UrlDaiDien { get; set; }

        public DateTime? NgayLap { get; set; } = DateTime.Now;

        public NguoiLapModel? NguoiLap { get; set; }
        public ThongTinNganHangModel? NganHang { get; set; } = null!;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public List<UploadedFileModel> AnhDaiDienFile { get; set; } = new List<UploadedFileModel>();
        public List<UploadedFileModel> MatTruocFile { get; set; } = new List<UploadedFileModel>();
        public List<UploadedFileModel> MatSauFile { get; set; } = new List<UploadedFileModel>();

        public PhongBanModel? PhongBan { get; set; }
        public string? MaPhongBan { get; set; }
        public ChucVuModel? ChucVu { get; set; }
        public string? MaChucVu { get; set; }
        public TrinhDoModel? TrinhDo { get; set; }
        public DanTocModel? DanToc { get; set; }
        public byte? TrangThai { get; set; }
        public string? MaNganHang { get; set; }
        public string? SoTaiKhoan { get; set; }
        public string? TenTaiKhoan { get; set; }
        public string? MaChiNhanh { get; set; }
        public string? DiaChiNganHang { get; set; }
        public string? TenDangNhap { get; set; }
        public int? UserId { get; set; } = null!;
        public string? MaSanGiaoDich { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
    }
    public class NguoiLapModel
    {
        public string MaNhanVien { get; set; } = null!;
        public string HoVaTen { get; set; } = null!;
        public string ChucVu { get; set; } = null!;
        public string LoaiUser { get; set; } = null!;
        public string MaSanGiaoDich { get; set; } = null!;
        public string TenSanGiaoDich { get; set; } = null!;
        public int SoLuongBookCanHo { get; set; } = 0;
    }
    public class TrinhDoModel
    {
        public string MaTrinhDo { get; set; } = null!;

        public string TenTrinhDo { get; set; } = null!;
    }

    public class PhongBanModel
    {
        public string MaPhongBan { get; set; } = null!;

        public string TenPhongBan { get; set; } = null!;
    }
    public class ChucVuModel
    {
        public string MaChucVu { get; set; } = null!;

        public string TenChucVu { get; set; } = null!;
    }

    public class DanTocModel
    {
        public string MaDanToc { get; set; } = null!;

        public string TenDanToc { get; set; } = null!;
    }

    public class ThongTinNganHangModel
    {
        public string MaNhanVien { get; set; } = null!;

        public string SoTaiKhoanNh { get; set; } = null!;

        public string TenTaiKhoanNh { get; set; } = null!;

        public string MaNganHang { get; set; } = null!;

        public string MaChiNhanh { get; set; } = null!;

        public string DiaChiNganHang { get; set; } = null!;

        public bool? TrangThaiChiLuong { get; set; }
    }
}
