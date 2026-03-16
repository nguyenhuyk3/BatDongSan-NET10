using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VTTGROUP.Application.LichSuThanhToan.Dto
{
    public class LichSuThanhToanRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập mã dự án")]
        [DefaultValue("")]
        public string maDuAn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng")]
        [DefaultValue("")]
        public string maKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã căn hộ")]
        [DefaultValue("")]
        public string maCanHo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã hợp đồng")]
        [DefaultValue("")]
        public string maHopDong { get; set; } = string.Empty;

        [DefaultValue("")]
        public string maGiaiDoanTT { get; set; } = string.Empty;
    }
}
