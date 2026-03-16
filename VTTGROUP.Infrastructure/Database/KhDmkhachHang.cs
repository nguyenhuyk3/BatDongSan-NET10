using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHang
{
    public string MaKhachHang { get; set; } = null!;

    public string TenKhachHang { get; set; } = null!;

    /// <summary>
    /// C: cá nhân,T: tổ chức
    /// </summary>
    public string? MaDoiTuongKhachHang { get; set; }

    public string? MaNhanVien { get; set; }

    public DateTime? NgayLap { get; set; }

    public virtual ICollection<KhDmkhachHangNguon> KhDmkhachHangNguons { get; set; } = new List<KhDmkhachHangNguon>();
}
