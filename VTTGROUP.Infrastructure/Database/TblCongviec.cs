using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblCongviec
{
    public string MaCongViec { get; set; } = null!;

    public string? TenCongViec { get; set; }

    public string? TenController { get; set; }

    public string? TenAction { get; set; }

    public bool HienThiTrenMenu { get; set; }

    public int? DoUuTien { get; set; }

    public string? GhiChu { get; set; }

    public string? MaCha { get; set; }

    public int? TrangThaiHienThi { get; set; }

    public string? TienTo { get; set; }

    public virtual ICollection<TblCongviecvavuviec> TblCongviecvavuviecs { get; set; } = new List<TblCongviecvavuviec>();

    public virtual ICollection<TblVuvieccuacongviec> TblVuvieccuacongviecs { get; set; } = new List<TblVuvieccuacongviec>();
}
