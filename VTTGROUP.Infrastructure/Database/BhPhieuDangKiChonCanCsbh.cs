using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class BhPhieuDangKiChonCanCsbh
{
    public string MaPhieuDangKy { get; set; } = null!;

    public string MaCsbh { get; set; } = null!;

    /// <summary>
    /// CSKM: Chính sách khuyến mãi (CS bán hàng); CSTT: Chính sách thanh toán
    /// </summary>
    public string LoaiCs { get; set; } = null!;

    public decimal? GiaTriCk { get; set; }

    public decimal? ThanhTienKmgiaBanDuocChon { get; set; }

    public decimal? ThanhTienKmgiaBanChinhThucDuocChon { get; set; }

    public bool? IsChon { get; set; }
}
