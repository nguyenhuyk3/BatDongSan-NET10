using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblVuviec
{
    public string MaVuViec { get; set; } = null!;

    public string TenVuViec { get; set; } = null!;

    public string? GhiChu { get; set; }

    public virtual ICollection<TblCongviecvavuviec> TblCongviecvavuviecs { get; set; } = new List<TblCongviecvavuviec>();

    public virtual ICollection<TblVuvieccuacongviec> TblVuvieccuacongviecs { get; set; } = new List<TblVuvieccuacongviec>();
}
