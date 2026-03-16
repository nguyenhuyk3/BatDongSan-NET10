using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblRefreshtoken
{
    public int Id { get; set; }

    public string? MaNhanVien { get; set; }

    public string? TenDangNhap { get; set; }

    public string? ToKen { get; set; }

    public DateTime? NgayHetHan { get; set; }

    public bool? IsRevoked { get; set; }

    public string? JwtId { get; set; }

    public int? UserId { get; set; }

    public virtual TblUser? User { get; set; }
}
