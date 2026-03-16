using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaGioHang
{
    public string MaGioHang { get; set; } = null!;

    public string NguoiLap { get; set; } = null!;

    public DateTime NgayLap { get; set; }

    public string MaDuAn { get; set; } = null!;

    public string MaDotMoBan { get; set; } = null!;

    public bool? IsGioHangChung { get; set; }

    /// <summary>
    /// 1: Gio hàng chung không chọn sàn giao dịch chỉ chọn sản phẩm từ đợt mở bán. 2: giỏ hàng riêng chọn sàn giao dịch và add sản phẩm vô từng sàn và mỗi sản phẩm chỉ thuộc mỗi sàn
    /// </summary>
    public int HinhThucMoBan { get; set; }

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public string? NoiDung { get; set; }

    public string? SoGioHang { get; set; }

    public decimal? GiaBan { get; set; }

    public string? MaSanGiaoDich { get; set; }

    public bool? IsDongGioHang { get; set; }

    public DateTime? NgayDongGioHang { get; set; }
}
