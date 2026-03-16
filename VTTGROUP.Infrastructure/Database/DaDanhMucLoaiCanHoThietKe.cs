namespace VTTGROUP.Infrastructure.Database;

public partial class DaDanhMucLoaiCanHoThietKe
{
    public int Id { get; set; }

    public string MaDuAn { get; set; } = null!;

    public string MaLoaiCanHo { get; set; } = null!;

    public string GhiChu { get; set; } = null!;
}
