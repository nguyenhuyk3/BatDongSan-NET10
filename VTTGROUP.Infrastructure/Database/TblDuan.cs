using System;
using System.Collections.Generic;

namespace VTTGROUP.Infrastructure.Database;

public partial class TblDuan
{
    public string MaDuAn { get; set; } = null!;

    public string TenDuAn { get; set; } = null!;

    public DateTime? NgayBatDau { get; set; }

    public DateTime? NgayKetThuc { get; set; }

    public string MaTienDo { get; set; } = null!;

    public string? GhiChu { get; set; }

    public string? MaNhanVien { get; set; }

    public DateTime? NgayLap { get; set; }

    /// <summary>
    /// Khi thời gian thực hiện dự án thay đổi thì thời gian công việc thay đổi theo.Khi bật cài đặt này thì, ví dụ: Thời gian thực hiện dự án là 10/03/2020 - 20/03/2020, công việc X thuộc dự án A có thời gian thực hiện là 11/03/2020 - 15/03/2020. Khi dự án A được tịnh tiến 3 ngày, tức thời gian thực hiện là 13/03/2020 – 20/03/2020, thì thời gian công việc X cũng tịnh tiến thêm 3 ngày, tức là 14/03/2020 - 15/03/2020.
    /// </summary>
    public bool? IsThoiGian { get; set; }

    /// <summary>
    /// Không cho phép người thực hiện công việc xem chéo các công việc khác.Khi bật cài đặt này thì, ví dụ: Dự án A gồm 2 công việc B và C, người thực hiện công việc B sẽ không được xem công việc C nếu người đó không phải là người thực hiện công việc C
    /// </summary>
    public bool? IsXemCheoCongViec { get; set; }

    public string? TrangThai { get; set; }

    public int Id { get; set; }
}
