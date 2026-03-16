using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblUser
{
    public int Id { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string MaNhanVien { get; set; } = null!;

    public bool TrangThai { get; set; }

    public DateTime NgayLap { get; set; }

    public string? ToKen { get; set; }

    public DateTime? NgayHetHangToken { get; set; }

    public string? NguoiLap { get; set; }

    /// <summary>
    /// &apos;NV&apos;: Nhân viên, &apos;SGD&apos;: Sàn giao dịch
    /// </summary>
    public string? LoaiUser { get; set; }

    public virtual ICollection<TblRefreshtoken> TblRefreshtokens { get; set; } = new List<TblRefreshtoken>();

    public virtual ICollection<TblUserthuocnhom> TblUserthuocnhoms { get; set; } = new List<TblUserthuocnhom>();
}
