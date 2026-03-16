using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucChinhSachThanhToanChiTiet
{
    public string MaPttt { get; set; } = null!;

    public string MaKyTt { get; set; } = null!;

    public string TenKyTt { get; set; } = null!;

    public string? MaGiaiDoanTt { get; set; }

    public int SoNgay { get; set; }

    public decimal TyLeTt { get; set; }

    public decimal TyLeVat { get; set; }

    public int ThuTuHienThi { get; set; }
}
