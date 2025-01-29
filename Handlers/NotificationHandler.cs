namespace UmbracoGaloSfera.Handlers
{
    using Microsoft.AspNetCore.Http;
    using Umbraco.Cms.Core.Models;
    using Umbraco.Cms.Core.Notifications;
    using Umbraco.Cms.Core.Services;
    using Microsoft.Extensions.Logging;
    using Umbraco.Cms.Core.Events;
    using System;
    using System.Collections.Generic;

    public class RestrictGalEditingHandler : INotificationHandler<ContentSavingNotification>, INotificationHandler<ContentDeletingNotification>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RestrictGalEditingHandler> _logger;
        private readonly IUserService _userService;

        public RestrictGalEditingHandler(IHttpContextAccessor httpContextAccessor, ILogger<RestrictGalEditingHandler> logger, IUserService userService)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _userService = userService;
        }

        // Handle pentru ContentSavingNotification
        public void Handle(ContentSavingNotification notification)
        {
            RestrictContent(notification.SavedEntities, notification, "editare");
        }

        // Handle pentru ContentDeletingNotification
        public void Handle(ContentDeletingNotification notification)
        {
            _logger.LogInformation("Handler-ul pentru ContentDeletingNotification a fost invocat.");
            RestrictContent(notification.DeletedEntities, notification, "ștergere");
        }

        // Metoda RestrictContent pentru a aplica restricțiile
        private void RestrictContent(IEnumerable<IContent> contentEntities, INotification notification, string action)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            // Verificăm dacă utilizatorul este autentificat
            if (httpContext == null || httpContext.User.Identity == null || !httpContext.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning($"Utilizator neautentificat încercând să {action} conținut.");
                return;
            }

            var userEmail = httpContext.User.Identity.Name;

            // Permisiuni nelimitate pentru un utilizator specific
            if (userEmail.Equals("dan.musetoiu@afir.info", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Iterăm prin fiecare entitate de conținut
            foreach (var content in contentEntities)
            {
                if (content.Id == 0 && action == "editare")
                {
                    _logger.LogInformation($"Permitem crearea unui nou {content.ContentType.Alias} de către {userEmail}");
                    continue; // Nu aplicăm restricțiile pentru crearea noilor entități
                }

                var creatorId = content.CreatorId;
                var creatorProfile = _userService.GetUserById(creatorId);
                string creatorEmail = creatorProfile?.Email ?? "";

                if (!string.IsNullOrEmpty(creatorEmail))
                {
                    _logger.LogInformation($"Emailul creatorului pentru {content.ContentType.Alias}: {creatorEmail}");
                }

                // Obținem email-ul utilizatorului
                string emailProperty = userEmail; // Poți folosi o metodă personalizată pentru a obține email-ul utilizatorului

                // Restricționăm acțiunile pentru BlogPost și Product
                if (content.ContentType.Alias == "blogpost" || content.ContentType.Alias == "product")
                {
                    if (string.IsNullOrWhiteSpace(creatorEmail) || !string.Equals(creatorEmail, emailProperty, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning($"Utilizatorul {emailProperty} nu are permisiunea de a {action} {content.ContentType.Alias} creat de {creatorEmail}.");

                        var eventMessage = new EventMessage(
                            $"Permisiune {action} {content.ContentType.Alias}",
                            $"Nu ai permisiunea de a {action} acest {content.ContentType.Alias}.",
                            EventMessageType.Error
                        );

                        if (action == "editare" && notification is ContentSavingNotification savingNotification)
                        {
                            savingNotification.CancelOperation(eventMessage);
                        }
                        else if (action == "ștergere" && notification is ContentDeletingNotification deletingNotification)
                        {
                            deletingNotification.CancelOperation(eventMessage);
                        }
                    }
                }
                // Restricționăm acțiunile pentru GAL
                else if (content.ContentType.Alias == "gal")
                {
                    if (string.IsNullOrWhiteSpace(emailProperty) || !string.Equals(emailProperty, userEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning($"Utilizatorul {emailProperty} nu are permisiunea de a {action} {content.ContentType.Alias} creat de {creatorEmail}.");

                        var eventMessage = new EventMessage(
                            $"Permisiune {action} {content.ContentType.Alias}",
                            $"Nu ai permisiunea de a {action} acest {content.ContentType.Alias}.",
                            EventMessageType.Error
                        );

                        if (action == "editare" && notification is ContentSavingNotification savingNotification)
                        {
                            savingNotification.CancelOperation(eventMessage);
                        }
                        else if (action == "ștergere" && notification is ContentDeletingNotification deletingNotification)
                        {
                            deletingNotification.CancelOperation(eventMessage);
                        }
                    }
                }
            }
        }
    }
}
