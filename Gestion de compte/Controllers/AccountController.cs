using AccountApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AccountApp.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _service;

        public AccountController(IAccountService service)
        {
            _service = service;
        }

        [HttpGet("value")]
        public async Task<IActionResult> GetAccountValue([FromQuery] DateTime date, [FromQuery] string filePath)
        {
            var dto = await _service.GetAccountValueAtAsync(date, filePath);
            return Ok(dto);
        }

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] string filePath)
        {
            var dtos = await _service.GetTopCategoriesAsync(filePath);
            return Ok(dtos);
        }
    }

}