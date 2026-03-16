using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class GioHangGroupService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<GioHangGroupService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public GioHangGroupService(AppDbContext context, ILogger<GioHangGroupService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }
    }
}
