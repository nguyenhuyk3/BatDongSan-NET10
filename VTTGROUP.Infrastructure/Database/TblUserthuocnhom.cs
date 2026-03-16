using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblUserthuocnhom
{
    public string MaNhomUser { get; set; } = null!;

    public int UserId { get; set; }

    public string? MaNv { get; set; }

    public virtual TblNhomuser MaNhomUserNavigation { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
