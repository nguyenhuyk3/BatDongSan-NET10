using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdHopDongTienDoThanhToan
{
    public int Id { get; set; }

    public string? MaHopDong { get; set; }

    public string? KyThanhToan { get; set; }

    public int? DotTt { get; set; }

    public string? NoiDungTt { get; set; }

    public int? DotThamChieu { get; set; }

    public int? SoKhoangCachNgay { get; set; }

    public decimal? TyLeTt { get; set; }

    public decimal? TyLeVat { get; set; }

    public decimal? SoTienTt { get; set; }

    public string? MaCstt { get; set; }

    public decimal? SoTienCanTruDaTt { get; set; }

    public decimal? SoTienPhaiThanhToan { get; set; }
}
