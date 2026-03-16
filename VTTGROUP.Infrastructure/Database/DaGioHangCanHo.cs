using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaGioHangCanHo
{
    public int Id { get; set; }

    public string MaGioHang { get; set; } = null!;

    public string MaCanHo { get; set; } = null!;

    public decimal? HeSoCanHo { get; set; }

    public decimal? DienTich { get; set; }

    public decimal? DienTichPhanBo { get; set; }

    public decimal? GiaBan { get; set; }

    public decimal? GiaBanPhanBo { get; set; }
}
