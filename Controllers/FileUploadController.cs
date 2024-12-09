using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.UmbracoContext;

namespace UmbracoGaloSfera.Controllers
{
    [Route("umbraco/api/[controller]/[action]")]
    public class FileUploadController : UmbracoApiController
    {
        private readonly IMediaService _mediaService;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public FileUploadController(IMediaService mediaService, IUmbracoContextFactory umbracoContextFactory)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _mediaService = mediaService;
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Niciun fișier nu a fost încărcat.");
            }

            // Creează directorul fizic dacă nu există
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Salvează fișierul fizic
            var filePath = Path.Combine(uploadsFolder, file.FileName);
            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // Verifică dacă fișierul este salvat corect
            Console.WriteLine($"Fișier salvat la: {filePath}");

            // Înregistrează fișierul în Media Library
            var mediaRoot = _mediaService.GetRootMedia().FirstOrDefault();  // Extrage folderul root de media
            var mediaFolder = mediaRoot ?? _mediaService.CreateMedia("Uploads", -1, "Folder"); // Crează folderul 'Uploads' dacă nu există

            if (mediaFolder == null)
            {
                return StatusCode(500, "Eroare la crearea directorului Media.");
            }

            // Creează un nod media pentru fișierul încărcat
            var mediaFile = _mediaService.CreateMedia(file.FileName, mediaFolder.Id, "File");

            // Setează calea fișierului în nodul Media
            mediaFile.SetValue("umbracoFile", $"/media/uploads/{file.FileName}");

            // Salvează nodul Media în Umbraco
            _mediaService.Save(mediaFile);

            //// Curăță cache-ul pentru Media pentru a reflecta modificările
            //using (var context = _umbracoContextFactory.EnsureUmbracoContext())
            //{
            //    context.UmbracoContext.MediaCache.Clear();
            //}

            // Debug: Verifică valorile setate
            Console.WriteLine($"Fișier Media creat: {file.FileName}, Cale: /media/uploads/{file.FileName}");

            return Ok(new { message = "Fișier încărcat cu succes și înregistrat în Media!", path = $"/media/uploads/{file.FileName}" });
        }




    }
}
