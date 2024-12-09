using Microsoft.AspNetCore.Mvc;
using UmbracoGaloSfera.Services;  // Importăm serviciul EmailService
using Microsoft.Extensions.Logging;  // Adaugăm referința pentru ILogger
using UmbracoGaloSfera.Models;

namespace UmbracoGaloSfera.Controllers
{
    [Route("Contact")]
    public class ContactController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<ContactController> _logger; // Adăugăm logger-ul

        // Injectăm EmailService și ILogger în constructor
        public ContactController(EmailService emailService, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;  // Inițializăm logger-ul
        }

        [HttpPost("SendEmail")]
        public async Task<IActionResult> SendEmail([FromBody] ContactFormModel form)
        {
            if (!ModelState.IsValid)
            {
                // Logăm erorile de validare pentru a vedea ce nu este corect
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError("Model state error: {ErrorMessage}", error.ErrorMessage);  // Logăm fiecare eroare
                }

                return BadRequest(new { success = false, message = "Datele trimise nu sunt valide." });
            }

            var success = await _emailService.SendEmailAsync(form.Name, form.Email, form.Message);

            if (success)
            {
                return Ok(new { success = true, message = "Mesajul a fost trimis cu succes!" });
            }
            else
            {
                return BadRequest(new { success = false, message = "A apărut o problemă la trimiterea mesajului." });
            }
        }
    }
}
