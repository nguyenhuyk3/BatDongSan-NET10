using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblVuvieccuacongviec
{
    public int VuViecCuaCongViecId { get; set; }

    public string? MaVuViec { get; set; }

    public string? MaCongViec { get; set; }

    public virtual TblCongviec? MaCongViecNavigation { get; set; }

    public virtual TblVuviec? MaVuViecNavigation { get; set; }
}
