using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.LaiPhatQuaHan.Dto;
using VTTGROUP.Application.LaiPhatQuaHan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Application.KyThanhToan;
using VTTGROUP.Application.KyThanhToan.Dto;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class KyThanhToanController : ControllerBase
    {
        private readonly IKyThanhToanService _lpService;
        public KyThanhToanController(IKyThanhToanService lpService)
        {
            _lpService = lpService;
        }
        [HttpPost("kythanhtoan")]
        public async Task<IActionResult> GetKyThanhToan([FromBody] KyThanhToanRequest request)
        {
            var listTD = await _lpService.GetKyThanhToanAsync(request.maDuAn, request.maKhachHang, request.maHopDong);
            var response = new ApiResponse<List<SystemKyThanhToan>>
            {
                Data = listTD,
                Message = "Dữ liệu kỳ thanh toán được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
