using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtGhiNhanLog
{
    public int Id { get; set; }

    public string? NoiDung { get; set; }

    public string? Controller { get; set; }
}
