using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiThietKeHinhAnh
{
    public int Id { get; set; }

    public string? MaLoaiThietKe { get; set; }

    public string? DuongDanLuuFileAnh { get; set; }

    public string? MaDuAn { get; set; }
}
