using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmnguonKhachHang
{
    public string MaNguonKhach { get; set; } = null!;

    public string TenNguonKhach { get; set; } = null!;

    public string? MaNguonCha { get; set; }

    public string? GhiChu { get; set; }
}
