using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtQuyTrinhDuyetBuocDuyet
{
    public int Id { get; set; }

    public int? IdQuyTrinh { get; set; }

    public string? MaBuocDuyet { get; set; }

    public int? ThuTu { get; set; }

    public string? NguoiDuyet { get; set; }
}
