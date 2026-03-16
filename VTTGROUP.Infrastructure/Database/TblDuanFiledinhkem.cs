using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanFiledinhkem
{
    public int Id { get; set; }

    public string MaDuAn { get; set; } = null!;

    public string TenFileHeThong { get; set; } = null!;

    public string TypeFile { get; set; } = null!;

    public string TenFile { get; set; } = null!;

    public string SizeFile { get; set; } = null!;
}
