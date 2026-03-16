using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TcTienDoThiCongDuAn
{
    public string MaPhieu { get; set; } = null!;

    public DateTime NgayLap { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string MaDuAn { get; set; } = null!;

    public string MaKhuVuc { get; set; } = null!;

    public string MaGiaiDoan { get; set; } = null!;

    public string GhiChu { get; set; } = null!;

    public int? MaQuiTrinhDuyet { get; set; }

    public int? TrangThaiDuyet { get; set; }

    public virtual DaDanhMucDuAn MaDuAnNavigation { get; set; } = null!;

    public virtual TcDmtienDoThiCongDuAn MaGiaiDoanNavigation { get; set; } = null!;

    public virtual KvDmkhuVuc MaKhuVucNavigation { get; set; } = null!;
}
