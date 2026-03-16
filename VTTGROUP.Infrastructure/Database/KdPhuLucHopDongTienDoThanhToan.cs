using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KdPhuLucHopDongTienDoThanhToan
{
    public int Id { get; set; }

    public string? MaPhuLuc { get; set; }

    public string? KyThanhToan { get; set; }

    public string? GiaiDoanTt { get; set; }

    public string? NoiDungTt { get; set; }

    public DateTime? NgayTt { get; set; }

    public double? TyLeTt { get; set; }

    public decimal? SoTienTt { get; set; }

    public string? MaCstt { get; set; }

    public int? DotTt { get; set; }
}
