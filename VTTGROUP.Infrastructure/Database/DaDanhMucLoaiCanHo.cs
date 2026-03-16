using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiCanHo
{
    public string MaLoaiCanHo { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string TenLoaiCanHo { get; set; } = null!;

    public decimal? DienTich { get; set; }

    public decimal? DienTichLotLong { get; set; }

    public decimal? DienTichSanVuon { get; set; }

    public int? SoPhongNgu { get; set; }

    public decimal? HeSoDienTich { get; set; }

    public string? MaLoaiThietKe { get; set; }

    public string? MoTa { get; set; }

    public string? HinhAnh { get; set; }
}
