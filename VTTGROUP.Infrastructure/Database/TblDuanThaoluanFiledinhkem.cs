using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanThaoluanFiledinhkem
{
    public int Id { get; set; }

    public int? IdthaoLuan { get; set; }

    public string TenFileHeThong { get; set; } = null!;

    public string TypeFile { get; set; } = null!;

    public string TenFile { get; set; } = null!;

    public string SizeFile { get; set; } = null!;
}
