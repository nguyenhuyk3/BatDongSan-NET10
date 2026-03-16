using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtPhieuXacNhanThanhToanChiTiet
{
    public int Id { get; set; }

    public string? MaPhieu { get; set; }

    public string? MaPhieuCongNo { get; set; }

    public string? IdChungTu { get; set; }

    public string? NoiDung { get; set; }

    public decimal? SoTien { get; set; }
}
