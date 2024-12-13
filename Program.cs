using UmbracoGaloSfera.Services; // Importăm serviciul EmailService
using Microsoft.Extensions.Configuration; // Pentru a adăuga configurările în builder

// Creăm builder-ul aplicației
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Adăugăm serviciul EmailService în containerul de dependență
builder.Services.AddTransient<EmailService>();
builder.Services.AddControllersWithViews();

// Configurăm CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://galosfera.go.ro")  // Permite cereri de la acest domeniu
               .AllowAnyMethod()  // Permite orice metodă HTTP (GET, POST, etc.)
               .AllowAnyHeader(); // Permite orice antet
    });
});

// Configurăm aplicația pentru a utiliza Umbraco
builder.CreateUmbracoBuilder()
    .AddBackOffice()  // Adaugă back office
    .AddWebsite()     // Adaugă partea de website
    .AddDeliveryApi() // Adaugă Delivery API
    .AddComposers()   // Adaugă componenetele Umbraco
    .Build();

// Construim aplicația
WebApplication app = builder.Build();

// Aplicăm CORS înainte de orice alt middleware
app.UseCors("AllowSpecificOrigin");

// Asigură-te că rutele MVC sunt configurate corect
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

// Asigurăm că Umbraco este inițializat corect
await app.BootUmbracoAsync();

// Setăm middleware pentru a folosi Umbraco
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();  // Permitem accesul la back office
        u.UseWebsite();     // Permitem accesul la website
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints(); // Permite accesul la endpoint-uri pentru instalare
        u.UseBackOfficeEndpoints(); // Permite endpoint-uri pentru back office
        u.UseWebsiteEndpoints();    // Permite endpoint-uri pentru website
    });

// Rulăm aplicația
await app.RunAsync();
