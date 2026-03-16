using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangNguon
{
    public string MaKhachHang { get; set; } = null!;

    public string MaKhachHangTam { get; set; } = null!;

    public DateTime? NgayCapNhat { get; set; }

    public virtual KhDmkhachHang MaKhachHangNavigation { get; set; } = null!;

    public virtual KhDmkhachHangTam MaKhachHangTamNavigation { get; set; } = null!;
}
