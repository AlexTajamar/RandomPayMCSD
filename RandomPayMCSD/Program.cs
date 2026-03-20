using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Repositories;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.LoginPath = "/RandomLogIn/Index";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); 
    options.AccessDeniedPath = "/RandomLogIn/ErrorAcceso";
});


string conecctionString = builder.Configuration.GetConnectionString("SqlRandom");
builder.Services.AddDbContext<RandomPayContext>
    (options => options.UseSqlServer(conecctionString));
builder.Services.AddScoped<IRepositoryUsuarios, RepositoryUsuarios>();
builder.Services.AddScoped<IRepositoryActividades, RepositoryActividades>();
builder.Services.AddScoped<IRepositoryGastos, RepositoryGastos>();
builder.Services.AddScoped<IRepositoryParticipantes, RepositoryParticipantes>();
builder.Services.AddTransient<IRepositoryDivisas, RepositoryDivisas>();
builder.Services.AddTransient<IRepositoryRepartos, RepositoryRepartos>();
builder.Services.AddScoped<IRepositoryListaCompra, RepositoryListaCompra>();
builder.Services.AddTransient<BalanceService>();
builder.Services.AddTransient<InvitationService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();


app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RandomLogIn}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
