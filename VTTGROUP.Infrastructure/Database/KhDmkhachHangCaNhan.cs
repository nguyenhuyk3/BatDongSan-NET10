using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class KhDmkhachHangCaNhan
{
    public int Id { get; set; }

    public string? MaKhachHang { get; set; }

    public string SoDt { get; set; } = null!;

    public string? Email { get; set; }

    public string? QuocTich { get; set; }

    public DateTime? NgaySinh { get; set; }

    public string? GioiTinh { get; set; }

    public string? SoCccd { get; set; }

    public DateTime? NgayCapCccd { get; set; }

    public string? NoiCapCccd { get; set; }

    public string? SoHoChieu { get; set; }

    public DateTime? NgayCapHoChieu { get; set; }

    public string? NoiCapHoChieu { get; set; }

    public string? DiaChiThuongTru { get; set; }

    public string? DiaChiHienNay { get; set; }

    /// <summary>
    /// 1: Độc thân, 2: Đã lập gia đình
    /// </summary>
    public int? TinhTrangHonNhan { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? IdlanDieuChinh { get; set; }

    public virtual KhDmkhachHang? MaKhachHangNavigation { get; set; }
}
