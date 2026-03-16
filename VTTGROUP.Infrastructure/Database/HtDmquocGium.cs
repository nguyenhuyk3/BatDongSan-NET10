using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDmquocGium
{
    public string MaQuocGia { get; set; } = null!;

    public string? TenQuocGia { get; set; }

    public virtual ICollection<KhDmkhachHangTam> KhDmkhachHangTams { get; set; } = new List<KhDmkhachHangTam>();
}
