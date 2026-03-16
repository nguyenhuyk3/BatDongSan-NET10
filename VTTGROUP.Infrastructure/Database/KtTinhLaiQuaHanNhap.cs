using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KtTinhLaiQuaHanNhap
{
    public int Id { get; set; }

    public string? MaCanHo { get; set; }

    public string? MaGiaiDoanTt { get; set; }

    public DateOnly? NgayBatDau { get; set; }

    public DateOnly? NgayKetThuc { get; set; }

    public double? SoTienTinhLai { get; set; }

    public int? SoNgayQuaHan { get; set; }

    public double? LaiSuatQuaHan { get; set; }

    public double? TienLai { get; set; }

    public double? DaDong { get; set; }

    public double? GiamTru { get; set; }
}
