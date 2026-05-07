using Microsoft.AspNetCore.Authentication.Cookies;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Repositories;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Azure Key Vault
string keyVaultUrl = builder.Configuration["KeyVaultUrl"] ?? "https://randompay.vault.azure.net/";
var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

string correoSecret = "";
string passSecret = "";

try 
{
    KeyVaultSecret secretEmail = await secretClient.GetSecretAsync("email");
    KeyVaultSecret secretPass = await secretClient.GetSecretAsync("pass");

    correoSecret = secretEmail.Value;
    passSecret = secretPass.Value;
} 
catch (Exception ex) 
{
    Console.WriteLine($"No se pudieron cargar los secretos del Key Vault: {ex.Message}");
}

// Sobrescribimos en configuración temporalmente sólo para que IConfiguration inyectado 
// siga teniendo acceso a "EmailSettings:Correo" desde donde ActividadesController lo lee.
// (Si no modificas la config ni cambias el controlador, ActividadesController no podrá leerlas)
builder.Configuration["EmailSettings:Correo"] = correoSecret;
builder.Configuration["EmailSettings:Password"] = passSecret;

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
