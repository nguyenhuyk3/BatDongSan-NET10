using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class HtDmnguoiDuyet
{
    public int Id { get; set; }

    public string? MaCongViec { get; set; }

    public string? MaPhieu { get; set; }

    public string? MaNhanVien { get; set; }

    public int? TrangThai { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public int? TrangThaiTraLai { get; set; }

    public string? NoiDung { get; set; }
}
