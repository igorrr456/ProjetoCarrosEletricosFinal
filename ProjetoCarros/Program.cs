using Microsoft.AspNetCore.Authentication.Cookies;
using ProjetoCarros.Interfaces;
using ProjetoCarros.Repositorio;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Usuario/Logar";
    options.AccessDeniedPath = "/Usuario/AcessoNegado";
});

builder.Services.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
builder.Services.AddScoped<ICarroRepositorio, CarroRepositorio>();
builder.Services.AddScoped<ICompraRepositorio, CompraRepositorio>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
