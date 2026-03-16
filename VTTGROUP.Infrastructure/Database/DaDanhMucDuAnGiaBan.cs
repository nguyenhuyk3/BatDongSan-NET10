using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucDuAnGiaBan
{
    public int Id { get; set; }

    public string? MaDuAn { get; set; }

    public decimal? DonGiaBanTb { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public string? NguoiCapNhat { get; set; }
}
