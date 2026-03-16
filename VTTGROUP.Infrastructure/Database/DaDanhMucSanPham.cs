using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucSanPham
{
    public string MaSanPham { get; set; } = null!;

    public string TenSanPham { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string? MaBlock { get; set; }

    public string? MaTang { get; set; }

    public string? MaTruc { get; set; }

    public string? MaLoaiDienTich { get; set; }

    public string? MaLoaiLayout { get; set; }

    public string? MaLoaiCan { get; set; }

    public decimal? DienTichTimTuong { get; set; }

    public decimal? DienTichThongThuy { get; set; }

    public decimal? DienTichSanVuon { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public string? HienTrangKd { get; set; }

    public string? LoaiSanPham { get; set; }

    public decimal? HeSoCanHo { get; set; }
}
