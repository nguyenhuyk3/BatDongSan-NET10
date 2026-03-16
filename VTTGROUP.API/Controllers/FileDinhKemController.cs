using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.KyThanhToan.Dto;
using VTTGROUP.Application.KyThanhToan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Application.FileDinhKem;
using VTTGROUP.Application.FileDinhKem.Dto;

namespace VTTGROUP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "ApiKeyPolicy")]
    public class FileDinhKemController : ControllerBase
    {
        private readonly IFileDinhKemService _lpService;
        public FileDinhKemController(IFileDinhKemService lpService)
        {
            _lpService = lpService;
        }
        [HttpPost("filedinhkem")]
        public async Task<IActionResult> GetFileDinhKem([FromBody] FileDinhKemRequest request)
        {
            var listTD = await _lpService.GetFileDinhKemAsync(request.maDuAn,request.maKhachHang,request.maHopDong,request.maKyThanhToan);
            var response = new ApiResponse<List<SystemFileDinhKem>>
            {
                Data = listTD,
                Message = "Dữ liệu file đính kèm được trả về thành công.",
                Status = "success"
            };
            return Ok(response);
        }
    }
}
