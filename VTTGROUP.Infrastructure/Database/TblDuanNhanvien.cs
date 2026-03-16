using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuanNhanvien
{
    public int Id { get; set; }

    public string MaNhanVien { get; set; } = null!;

    /// <summary>
    /// 0: Người Quản Trị, 1: Người Thực Hiện
    /// </summary>
    public int Loai { get; set; }

    public string? MaDuAn { get; set; }
}
