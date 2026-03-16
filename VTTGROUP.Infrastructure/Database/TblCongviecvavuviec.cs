using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblCongviecvavuviec
{
    public string MaCongViec { get; set; } = null!;

    public string MaVuViec { get; set; } = null!;

    public string MaNhomUser { get; set; } = null!;

    public virtual TblCongviec MaCongViecNavigation { get; set; } = null!;

    public virtual TblNhomuser MaNhomUserNavigation { get; set; } = null!;

    public virtual TblVuviec MaVuViecNavigation { get; set; } = null!;
}
