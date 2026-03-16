using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmloaiCard
{
    public string MaLoaiIdCard { get; set; } = null!;

    public string TenLoaiIdCard { get; set; } = null!;

    public bool? IsBatBuocNgayCapNoiCap { get; set; }

    public virtual ICollection<KhDmkhachHangTam> KhDmkhachHangTams { get; set; } = new List<KhDmkhachHangTam>();
}
