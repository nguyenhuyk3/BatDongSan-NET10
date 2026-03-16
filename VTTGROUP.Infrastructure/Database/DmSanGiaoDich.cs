using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DmSanGiaoDich
{
    public string MaSanGiaoDich { get; set; } = null!;

    public string? TenSanGiaoDich { get; set; }

    public string? GhiChu { get; set; }

    public string? DiaChi { get; set; }

    public string? DienThoai { get; set; }

    public string? Email { get; set; }

    /// <summary>
    /// 1: Đang mở, 0 đang đóng
    /// </summary>
    public int? TrangThai { get; set; }

    public string? NguoiLap { get; set; }

    public DateTime? NgayLap { get; set; }

    public virtual ICollection<KhDmkhachHangTam> KhDmkhachHangTams { get; set; } = new List<KhDmkhachHangTam>();
}
