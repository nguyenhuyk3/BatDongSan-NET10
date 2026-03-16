using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtFileDinhKem
{
    public int Id { get; set; }

    public string MaPhieu { get; set; } = null!;

    public string? Controller { get; set; }

    public string? AcTion { get; set; }

    public string TenFileDinhKem { get; set; } = null!;

    public string TenFileDinhKemLuu { get; set; } = null!;

    public DateTime NgayLap { get; set; }

    public string MaNhanVien { get; set; } = null!;

    public string TenNhanVien { get; set; } = null!;

    public string? TaiLieuUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? FileSize { get; set; }

    public string? FileType { get; set; }

    public string? FullDomain { get; set; }
}
