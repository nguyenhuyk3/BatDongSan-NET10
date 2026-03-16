using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DmSanGiaoDichNhanVien
{
    public int Id { get; set; }

    public string? MaSanGiaoDich { get; set; }

    public string? MaNhanVien { get; set; }

    public string? GhiChu { get; set; }

    public virtual DmSanGiaoDich? MaSanGiaoDichNavigation { get; set; }
}
