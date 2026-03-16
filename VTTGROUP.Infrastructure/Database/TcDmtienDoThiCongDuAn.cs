using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TcDmtienDoThiCongDuAn
{
    public string MaGiaiDoan { get; set; } = null!;

    public string TenGiaiDoan { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string MaKhuVuc { get; set; } = null!;

    public DateTime NgayBatDau { get; set; }

    public DateTime NgayKetThuc { get; set; }

    public string GhiChu { get; set; } = null!;

    public DateTime NgayHoanThanh { get; set; }

    public bool? IsHoanThanh { get; set; }

    public virtual ICollection<TcTienDoThiCongDuAn> TcTienDoThiCongDuAns { get; set; } = new List<TcTienDoThiCongDuAn>();
}
