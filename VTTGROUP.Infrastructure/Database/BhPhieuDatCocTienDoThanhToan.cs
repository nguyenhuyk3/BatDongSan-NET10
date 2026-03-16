using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDatCocTienDoThanhToan
{
    public int Id { get; set; }

    public string? MaPhieuDc { get; set; }

    public string? MaCstt { get; set; }

    public int? DotTt { get; set; }

    public string? NoiDungTt { get; set; }

    public string? MaKyTt { get; set; }

    public int? DotThamChieu { get; set; }

    public int? SoKhoangCachNgay { get; set; }

    public decimal? TyLeTt { get; set; }

    public decimal? TyLeTtvat { get; set; }

    public decimal? SoTienThanhToan { get; set; }

    public decimal? SoTienCanTruDaTt { get; set; }

    public decimal? SoTienChuyenDoiBooking { get; set; }

    public decimal? SoTienPhaiThanhToan { get; set; }

    public DateTime? NgayThanhToan { get; set; }
}
