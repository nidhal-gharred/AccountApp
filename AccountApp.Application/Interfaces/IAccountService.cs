using AccountApp.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountApp.Application.Interfaces
{
    public interface IAccountService
    {
        Task<AccountValueDto> GetAccountValueAtAsync(DateTime date, string filePath);
        Task<List<CategoryTotalDto>> GetTopCategoriesAsync(string filePath);
    }
}
