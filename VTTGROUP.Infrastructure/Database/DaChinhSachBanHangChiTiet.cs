using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaChinhSachBanHangChiTiet
{
    public int Id { get; set; }

    public string? MaPhieu { get; set; }

    public string? MaCsbh { get; set; }

    public string? TenCsbh { get; set; }

    public string? MaHinhThucKm { get; set; }

    public string? MaLoaiDieuKienKm { get; set; }

    public int? SoLuongKm { get; set; }

    public DateTime? TuNgay { get; set; }

    public DateTime? DenNgay { get; set; }

    public decimal? GiaTriKm { get; set; }

    public int? SttUuTien { get; set; }
}
