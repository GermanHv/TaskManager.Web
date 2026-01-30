using TaskManager.Web.Extensions;
using TaskManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddApiClients();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

//Esa línea define la ruta “clásica” (MVC) que ASP.NET Core usa para decidir qué controlador y qué acción ejecutar cuando llega una petición HTTP. Si llega una URL y no coincide con nada más específico,intenta interpretarla como:Controlador / Acción / Id opcional.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
