using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.Menu
{
    public interface IMenuService
    {
        Task<List<SystemMenu>> GetMenuByUserAsync(string username);
    }
}
