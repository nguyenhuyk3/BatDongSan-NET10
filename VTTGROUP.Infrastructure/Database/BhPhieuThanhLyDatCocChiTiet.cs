using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuThanhLyDatCocChiTiet
{
    public int Id { get; set; }

    public string? MaPhieuTl { get; set; }

    public string? MaCstt { get; set; }

    public int? DotTt { get; set; }

    public string? NoiDungTt { get; set; }

    public string? MaKyTt { get; set; }

    public decimal? SoTienThanhToan { get; set; }

    public decimal? SoTienDaTt { get; set; }

    public decimal? SoTienConLai { get; set; }
}
