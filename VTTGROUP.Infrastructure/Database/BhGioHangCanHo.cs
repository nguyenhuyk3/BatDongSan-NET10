using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhGioHangCanHo
{
    public int Id { get; set; }

    public string? MaPhieuGioHang { get; set; }

    public string? MaCanHo { get; set; }

    public decimal? HeSoCanHo { get; set; }

    public decimal? DienTichCanHo { get; set; }

    public decimal? DienTichPhanBo { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaBanSauPhanBo { get; set; }
}
