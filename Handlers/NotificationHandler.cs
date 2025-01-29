namespace UmbracoGaloSfera.Handlers
{
    using Microsoft.AspNetCore.Http;
    using Umbraco.Cms.Core.Events;
    using Umbraco.Cms.Core.Notifications;

    public class RestrictGalEditingHandler : INotificationHandler<ContentSavingNotification>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RestrictGalEditingHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Handle(ContentSavingNotification notification)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true) return;

            var userEmail = httpContext.User.Identity.Name;

            foreach (var content in notification.SavedEntities)
            {
                if (content.ContentType.Alias == "gal" || content.ContentType.Alias == "Blogpost" || content.ContentType.Alias == "product")
                {
                    var emailProperty = content.GetValue<string>("EmailGal");

                    if (!string.Equals(emailProperty, userEmail, StringComparison.OrdinalIgnoreCase))
                    {
                        notification.CancelOperation(new EventMessage("Restricție acces", "Nu ai permisiunea de a edita acest element.", EventMessageType.Error));

                    }
                }
            }
        }
    }
}
