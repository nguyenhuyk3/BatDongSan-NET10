using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangHinhAnhDinhKem
{
    public string MaKhachHang { get; set; } = null!;

    public string MaHinhAnh { get; set; } = null!;

    public DateTime? NgayCapNhat { get; set; }
}
