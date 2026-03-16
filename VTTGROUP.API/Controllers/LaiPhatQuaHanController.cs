using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.LichSuThanhToan.Dto;
using VTTGROUP.Application.LichSuThanhToan;
using VTTGROUP.Application.LaiPhatQuaHan;
using VTTGROUP.Application.LaiPhatQuaHan.Dto;
using VTTGROUP.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class LaiPhatQuaHanController : ControllerBase
    {
        private readonly ILaiPhatQuaHanService _lpService;
        public LaiPhatQuaHanController(ILaiPhatQuaHanService lpService)
        {
            _lpService = lpService;
        }
        [HttpPost("laiphatquahan")]
        public async Task<IActionResult> GetLaiPhatQuaHan([FromBody] LaiPhatQuaHanRequest request)
        {
            var listTD = await _lpService.GetLaiPhatQuaHanAsync(request.maDuAn, request.maKhachHang, request.maCanHo, request.maGiaiDoanTT);
            var response = new ApiResponse<List<SystemLaiPhatQuaHan>>
            {
                Data = listTD,
                Message = "Dữ liệu lãi phạt quá hạn được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
