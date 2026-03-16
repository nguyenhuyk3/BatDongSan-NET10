using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucDuAn
{
    public string MaDuAn { get; set; } = null!;

    public string TenDuAn { get; set; } = null!;

    public string? DiaChi { get; set; }

    public string? TinhThanh { get; set; }

    public string? XaPhuong { get; set; }

    public string GhiChu { get; set; } = null!;

    /// <summary>
    /// 1: Đang mở bán, 2: Sắp mở bán, 3: Đóng dự án
    /// </summary>
    public int TrangThai { get; set; }

    public decimal? TongDienTichDuAn { get; set; }

    public virtual ICollection<TcTienDoThiCongDuAn> TcTienDoThiCongDuAns { get; set; } = new List<TcTienDoThiCongDuAn>();
}
