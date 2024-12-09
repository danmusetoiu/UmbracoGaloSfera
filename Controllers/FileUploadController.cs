using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Web.Common.Controllers;

namespace UmbracoGaloSfera.Controllers
{
    [Route("umbraco/api/[controller]/[action]")]
    public class FileUploadController : UmbracoApiController
    {
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Niciun fișier nu a fost încărcat.");
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "uploads");

            // Creează directorul dacă nu există
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);

            // Salvează fișierul pe disc
            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return Ok(new { message = "Fișier încărcat cu succes!", path = $"/media/uploads/{file.FileName}" });
        }
    }
}
