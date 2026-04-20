using Microsoft.AspNetCore.Authentication.Cookies;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Repositories;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

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

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? "https://apirandompayarg-ane5bcaxexevatff.germanywestcentral-01.azurewebsites.net";

builder.Services.AddHttpClient<RepositoryUsuarios>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryActividades>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryGastos>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryParticipantes>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryDivisas>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryRepartos>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<RepositoryListaCompra>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<AuthApiService>(c => c.BaseAddress = new Uri(baseUrl));
builder.Services.AddHttpClient<UsuarioApiService>(c => c.BaseAddress = new Uri(baseUrl));

builder.Services.AddScoped<IRepositoryUsuarios>(sp => sp.GetRequiredService<RepositoryUsuarios>());
builder.Services.AddScoped<IRepositoryActividades>(sp => sp.GetRequiredService<RepositoryActividades>());
builder.Services.AddScoped<IRepositoryGastos>(sp => sp.GetRequiredService<RepositoryGastos>());
builder.Services.AddScoped<IRepositoryParticipantes>(sp => sp.GetRequiredService<RepositoryParticipantes>());
builder.Services.AddScoped<IRepositoryDivisas>(sp => sp.GetRequiredService<RepositoryDivisas>());
builder.Services.AddScoped<IRepositoryRepartos>(sp => sp.GetRequiredService<RepositoryRepartos>());
builder.Services.AddScoped<IRepositoryListaCompra>(sp => sp.GetRequiredService<RepositoryListaCompra>());
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
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=RandomLogIn}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
