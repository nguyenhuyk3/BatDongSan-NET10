using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database2.Entities;

public partial class TC_PhieuPhaiThu
{
    public string maPhieu { get; set; } = null!;

    public DateTime? ngayLap { get; set; }

    public string? nguoiLap { get; set; }

    public DateTime? ngayHachToan { get; set; }

    public string? maDoiTuong { get; set; }

    public string? tenDoiTuong { get; set; }

    public string? diaChi { get; set; }

    public string? noiDung { get; set; }

    public string? soPhieuThanhToan { get; set; }

    public decimal? soTien { get; set; }

    public string? maCongTrinh { get; set; }

    public string? maCongViecChungTu { get; set; }

    public string? soHopDong { get; set; }

    public DateTime? ngayDong { get; set; }

    public string? maNhanVienDong { get; set; }

    public string? lyDoDong { get; set; }

    public bool? trangThaiDong { get; set; }
}
