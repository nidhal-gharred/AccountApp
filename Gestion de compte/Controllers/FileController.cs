using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AccountApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");

        public FileController()
        {
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadCsv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty.");

            var filePath = Path.Combine(_uploadPath, file.FileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { FilePath = filePath });
        }
    }
}
