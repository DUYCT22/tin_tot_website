using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TinTot.Application.DTOs;

namespace TinTot.Application.Interfaces.Home
{
    public interface IHomeQueryService
    {
        Task<HomePageDto> GetHomePageDataAsync(int? currentUserId, int take = 6);
    }
}
