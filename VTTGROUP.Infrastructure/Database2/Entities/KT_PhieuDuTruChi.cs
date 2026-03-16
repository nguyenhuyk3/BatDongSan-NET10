using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database2.Entities;

public partial class KT_PhieuDuTruChi
{
    public string maPhieuDuTruChi { get; set; } = null!;

    public string? soPhieuDuTruChi { get; set; }

    public DateTime? ngayLap { get; set; }

    public DateTime? ngayHachToan { get; set; }

    public string? maDoiTuong { get; set; }

    public string? tenDoiTuong { get; set; }

    public string? diaChi { get; set; }

    public string? noiDung { get; set; }

    public string? KemTheo { get; set; }

    public string? maTieuChiDuyetChi { get; set; }

    public string? maTienTe { get; set; }

    public decimal? tyGia { get; set; }

    public string nguoiLap { get; set; } = null!;

    /// <summary>
    /// 0: chi khác, 1: chi đơn hàng, 2: chi hợp đồng
    /// </summary>
    public short? loaiDuTruChi { get; set; }

    public string? maPhuongThucThanhToan { get; set; }

    public bool? trangThai { get; set; }

    public string? chuoiHoadon { get; set; }

    public string? loaiChi { get; set; }

    public string? nguoiTiepNhan { get; set; }

    public string? maNganHang { get; set; }

    public string? maChiNhanh { get; set; }

    public string? soTaiKhoan { get; set; }

    public string? tenTaiKhoan { get; set; }

    public bool? isDaChiDu { get; set; }

    public string? soPhieuThanhToan { get; set; }

    public decimal? soTien { get; set; }

    public string? maLoaiChiPhi { get; set; }

    public string? maCongTrinh { get; set; }

    public DateOnly? hanThanhToan { get; set; }

    public string? maCongViecChungTu { get; set; }

    public string? soHopDong { get; set; }

    public bool? idDongPhieu { get; set; }

    public string? lyDoDong { get; set; }

    public string? nguoiDong { get; set; }
}
