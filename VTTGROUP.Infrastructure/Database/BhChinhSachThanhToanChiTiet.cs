using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhChinhSachThanhToanChiTiet
{
    public int Id { get; set; }

    public string? MaCstt { get; set; }

    /// <summary>
    /// Tự tăng theo từng chính sách
    /// </summary>
    public int? DotTt { get; set; }

    public string? NoiDungTt { get; set; }

    public string? MaKyTt { get; set; }

    public int? SoKhoangCachNgay { get; set; }

    public int? DotThamChieu { get; set; }

    public decimal? TyLeTtdatCoc { get; set; }

    public bool? IsCongNoDc { get; set; }

    public decimal? TyLeTtdatCocVat { get; set; }
}
