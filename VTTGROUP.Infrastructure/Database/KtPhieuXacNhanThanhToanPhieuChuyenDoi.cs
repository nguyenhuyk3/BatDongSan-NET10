using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtPhieuXacNhanThanhToanPhieuChuyenDoi
{
    public string MaPhieu { get; set; } = null!;

    public string MaPhieuNguon { get; set; } = null!;

    public decimal? SoTienChuyenDoi { get; set; }
}
