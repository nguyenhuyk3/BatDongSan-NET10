using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtLichSuThanhToan
{
    public int Id { get; set; }

    public string? MaHopDong { get; set; }

    public string? SoChungTu { get; set; }

    public DateOnly? NgayChungTu { get; set; }

    public DateTime? NgapLap { get; set; }

    public string? NguoiLap { get; set; }

    public string? NoiDung { get; set; }

    public double? SoTien { get; set; }

    public string? MaDuAn { get; set; }

    public string? MaGiaiDoanTt { get; set; }
}
