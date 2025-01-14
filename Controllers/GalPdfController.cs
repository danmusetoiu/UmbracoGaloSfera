using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using System.Linq;
using Umbraco.Cms.Core.Web;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Web.Common.UmbracoContext;
using Umbraco.Cms.Core;


namespace UmbracoGaloSfera.Controllers
{
    // Controllerul pentru paginile de content
    public class ContentPageController : RenderController
    {
        public ContentPageController(
            ILogger<ContentPageController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
        }

        // Metoda pentru afișarea paginii
        public override IActionResult Index()
        {
            var contentPage = CurrentPage as ContentPage;

            if (contentPage == null)
            {
                return NotFound();
            }

            var rootContent = UmbracoContext.Content.GetAtRoot();

            var listaGaluri = rootContent
                .SelectMany(content => content.Descendants())  // Aplica Descendants pentru fiecare nod
                .OfType<ListaGaluri>()  // Filtrăm pentru tipul ListaGaluri
                .FirstOrDefault();  // Luăm primul nod care se potrivește (dacă există)

            ViewData["ListaGaluri"] = listaGaluri;

            return View("contentPage", contentPage);
        }
    }

    // Controller API pentru generarea PDF-ului precompletat
    [Route("umbraco/api/contentpage")]
    public class ContentPageApiController : UmbracoApiController
    {
        private readonly ILogger<ContentPageApiController> _logger;
        private readonly IUmbracoContextFactory _umbracoContextFactory;

        public ContentPageApiController(ILogger<ContentPageApiController> logger, IUmbracoContextFactory umbracoContextFactory)
        {
            _logger = logger;
            _umbracoContextFactory = umbracoContextFactory;
        }

        

        [HttpGet("GeneratePdf")]
        public IActionResult GeneratePdf(string galName)
        {
            try
            {
                _logger.LogInformation($"Cerere de generare PDF pentru GAL: {galName}");

                // Calea către PDF-ul șablon
                string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "4dfddkqy", "formular_230_curat.pdf");

                // Verifică dacă fișierul șablon există
                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogError($"Șablonul PDF nu a fost găsit: {templatePath}");
                    return NotFound("Șablonul PDF nu a fost găsit.");
                }

                // Calea către fișierul generat
                string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "temp", $"{galName}_230.pdf");

                // Creează directorul pentru fișierul generat, dacă nu există
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                FileStream fs = null;
                PdfReader reader = null;
                PdfStamper stamper = null;

                using (var umbracoContextReference = _umbracoContextFactory.EnsureUmbracoContext())
                {
                    var umbracoContext = umbracoContextReference.UmbracoContext;

                    // Căutăm GAL-ul pe baza numelui
                    var gal = umbracoContext.Content
                    .GetAtRoot()  // Începem de la rădăcina site-ului
                    .SelectMany(x => x.Descendants())  // Aplicăm Descendants pentru fiecare element rădăcină
                    .Where(x => x.Name == galName) // Căutăm pe baza numelui GAL-ului
                    .FirstOrDefault(); // Dacă există, îl luăm pe primul

                    var cif = gal.GetProperty("cIFGAL").GetValue().ToString();
                    var iban = gal.GetProperty("IBAN").GetValue().ToString();

                    if (gal == null)
                    {
                        _logger.LogError($"GAL-ul cu numele '{galName}' nu a fost găsit.");
                        return NotFound($"GAL-ul cu numele '{galName}' nu a fost găsit.");
                    }

                    try
                    {
                        // Inițializează FileStream și PdfReader
                        fs = new FileStream(outputPath, FileMode.Create);
                        reader = new PdfReader(templatePath);

                        // Inițializează PdfStamper
                        stamper = new PdfStamper(reader, fs);

                        // Obține formularul PDF
                        AcroFields form = stamper.AcroFields;

                        // Completează câmpul "Precompletat_denumire" cu numele GAL-ului
                        if (form.GetField("Precompletat_denumire") != null)
                        {
                            form.SetField("Precompletat_denumire", gal.Name);
                        }

                        // Completează câmpul IBAN (dacă există în formular)
                        if (form.GetField("iban_precompletat") != null)
                        {
                            form.SetField("iban_precompletat", iban); // Adaugă IBAN-ul GAL-ului
                        }

                        // Completează câmpul CIF (dacă există în formular)
                        if (form.GetField("Precompletat_CIF") != null)
                        {
                            form.SetField("Precompletat_CIF", cif); // Adaugă CIF-ul GAL-ului
                        }

                        // Setează câmpurile ca neschimbabile după completare
                        stamper.FormFlattening = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Eroare la completarea PDF-ului: {ex.Message}");
                        return BadRequest($"Eroare la completarea PDF-ului: {ex.Message}");
                    }
                    finally
                    {
                        // Asigură-te că eliberezi resursele
                        stamper?.Close();
                        reader?.Close();
                        fs?.Close();
                    }
                }


                // Citește fișierul generat și îl returnează pentru descărcare
                byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);
                return File(fileBytes, "application/pdf", $"{galName}_230.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la generarea PDF-ului: {ex.Message}");
                return BadRequest($"Eroare la generarea PDF-ului: {ex.Message}");
            }
        }

    }

    // ViewModel pentru a include atât ContentPage cât și ListaGaluri
    public class ContentAndListaGaluriViewModel
    {
        public ContentPage ContentPage { get; set; }
        public ListaGaluri ListaGaluri { get; set; }
    }
}
