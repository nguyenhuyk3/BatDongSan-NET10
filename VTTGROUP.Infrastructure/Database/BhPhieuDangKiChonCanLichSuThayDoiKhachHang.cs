using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDangKiChonCanLichSuThayDoiKhachHang
{
    public int Id { get; set; }

    public string? MaPhieuDangKy { get; set; }

    public string? MaPhieuBooking { get; set; }

    public string? MaKhachHangTam { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? NguoiCapNhat { get; set; }
}
