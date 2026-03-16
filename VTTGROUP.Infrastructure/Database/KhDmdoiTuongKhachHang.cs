using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmdoiTuongKhachHang
{
    public string MaDoiTuongKhachHang { get; set; } = null!;

    public string TenDoiTuongKhachHang { get; set; } = null!;

    public bool? IsCheckGioiTinh { get; set; }

    public bool? IsCheckNgayCapNoiCapIdCard { get; set; }

    public bool? IsHienThiNguoiDaiDien { get; set; }

    public bool? IsHienThiNgaySinh { get; set; }

    public bool? IsHienThiQuocTich { get; set; }

    public bool? IsHienThiDiaChiThuongTru { get; set; }
}
