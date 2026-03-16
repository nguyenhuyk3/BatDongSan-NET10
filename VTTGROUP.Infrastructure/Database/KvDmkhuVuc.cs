using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KvDmkhuVuc
{
    public string MaKhuVuc { get; set; } = null!;

    public string TenKhuVuc { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string GhiChu { get; set; } = null!;

    public virtual ICollection<TcTienDoThiCongDuAn> TcTienDoThiCongDuAns { get; set; } = new List<TcTienDoThiCongDuAn>();
}
