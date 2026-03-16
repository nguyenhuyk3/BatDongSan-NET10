using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucViewTruc
{
    public string MaDuAn { get; set; } = null!;

    public string MaBlock { get; set; } = null!;

    public string MaTruc { get; set; } = null!;

    public string TenTruc { get; set; } = null!;

    public decimal? HeSoTruc { get; set; }

    public int? ThuTuHienThi { get; set; }

    public string? MaViewMatKhoi { get; set; }

    public string? MaHuong { get; set; }

    public string? MaLoaiView { get; set; }

    public string? MaLoaiGoc { get; set; }

    public string? MaViTri { get; set; }
}
