﻿namespace UmbracoGaloSfera.Composers // Asigură-te că se potrivește cu structura ta de foldere
{
    using Microsoft.Extensions.DependencyInjection;
    using Umbraco.Cms.Core.DependencyInjection;
    using Umbraco.Cms.Core.Composing;
    using Umbraco.Cms.Core.Notifications;
    using UmbracoGaloSfera.Handlers; // Importă handler-ul dacă este în alt folder

    public class RestrictGalEditingComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.AddNotificationHandler<ContentSavingNotification, RestrictGalEditingHandler>();
        }
    }
}
