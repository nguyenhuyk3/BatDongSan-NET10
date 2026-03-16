using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblNhomuser
{
    public string MaNhomUser { get; set; } = null!;

    public string? TenNhomUser { get; set; }

    public string? GhiChu { get; set; }

    public bool? TrangThai { get; set; }

    public virtual ICollection<TblCongviecvavuviec> TblCongviecvavuviecs { get; set; } = new List<TblCongviecvavuviec>();

    public virtual ICollection<TblUserthuocnhom> TblUserthuocnhoms { get; set; } = new List<TblUserthuocnhom>();
}
