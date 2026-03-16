using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuGiuCho
{
    public string MaPhieu { get; set; } = null!;

    /// <summary>
    /// tự điền theo quy tắc: Mã dự án +&apos;-&apos;+Mã Đợt +&apos;-&apos; +&apos;4 stt tăng của đợt&apos;
    /// </summary>
    public string? SoPhieu { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? DotMoBan { get; set; }

    public string? MaKhachHangTam { get; set; }

    public decimal? SoTienGiuCho { get; set; }

    public string? MaSanMoiGioi { get; set; }

    public string? TenNhanVienMg { get; set; }

    public string? NoiDung { get; set; }

    public bool? IsxacNhan { get; set; }

    public string? MaLoaiThietKe { get; set; }

    public string? MaMatKhoi { get; set; }

    public int? SoTtbooking { get; set; }

    public string? MaPhieuTh { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }
}
