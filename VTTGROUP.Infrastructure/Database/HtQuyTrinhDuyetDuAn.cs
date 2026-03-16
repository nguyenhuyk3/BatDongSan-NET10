using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtQuyTrinhDuyetDuAn
{
    public int Id { get; set; }

    public int? IdQuyTrinh { get; set; }

    public string? MaDuAn { get; set; }
}
