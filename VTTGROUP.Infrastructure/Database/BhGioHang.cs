using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhGioHang
{
    public string MaPhieu { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaDotMoBan { get; set; }

    public string? MaSoGioHang { get; set; }

    public decimal? GiaBan { get; set; }

    /// <summary>
    /// 1: giỏ hàng riêng, 0: giỏ hàng chung
    /// </summary>
    public bool? LoaiGioHang { get; set; }

    public string? MaSanGiaoDich { get; set; }

    public string? NoiDung { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public bool? IsDong { get; set; }

    public DateTime? NgayDong { get; set; }

    public string? NguoiDong { get; set; }

    public string? MaPhieuDuyetGia { get; set; }

    public string? MaPhieuKh { get; set; }
}
