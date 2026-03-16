using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanLichsuhoatdong
{
    public int Id { get; set; }

    public string? MaDuAn { get; set; }

    public string? TrangThai { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? MaNhanVien { get; set; }
}
