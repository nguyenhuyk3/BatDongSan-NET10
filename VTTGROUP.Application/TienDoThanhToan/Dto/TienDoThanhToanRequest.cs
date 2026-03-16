using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VTTGROUP.Application.TienDoThanhToan.Dto
{
    public class TienDoThanhToanRequest
    {
        [DefaultValue("")]
        public string maDuAn { get; set; } = string.Empty;
        [DefaultValue("")]
        public string? maHopDong { get; set; } = string.Empty;
        [Required(ErrorMessage = "Vui lòng nhập mã khách hàng")]
        [DefaultValue("")]
        public string maKhachHang { get; set; } = string.Empty;
        [DefaultValue("")]
        public string? QSearch { get; set; } = string.Empty;
        public int? trangThaiThanhToan { get; set; } = null;
    }
}
