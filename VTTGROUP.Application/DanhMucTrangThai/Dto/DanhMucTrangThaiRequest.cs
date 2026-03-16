using System.ComponentModel;

namespace VTTGROUP.Application.DanhMucTrangThai.Dto
{
    public class DanhMucTrangThaiRequest
    {
        [DefaultValue(1)]
        public int? pageIndex { get; set; } = 1;
        [DefaultValue(10)]
        public int? numOfPage { get; set; } = 10;
    }
}
