using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.TienDoThanhToan;
using VTTGROUP.Application.TienDoThanhToan.Dto;
using VTTGROUP.Domain.Entities;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class TienDoThanhToanController : ControllerBase
    {
        private readonly ITienDoThanhToanService _tdService;
        public TienDoThanhToanController(ITienDoThanhToanService tdService)
        {
            _tdService = tdService;
        }
        [HttpPost("tiendothanhtoan")]
        public async Task<IActionResult> GetTienDoThanhToan([FromBody] TienDoThanhToanRequest request)
        {
            var listTD = await _tdService.GetTienDoThanhToanAsync(request.maDuAn, request.maHopDong ?? string.Empty, request.maKhachHang, request.QSearch ?? string.Empty,request.trangThaiThanhToan);
            var response = new ApiResponse<List<SystemTienDoThanhToan>>
            {
                Data = listTD,
                Message = "Dữ liệu tiến độ thanh toán được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
