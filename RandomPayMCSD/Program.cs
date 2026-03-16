using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Interfaces;
using RandomPayMCSD.Repositories;
using RandomPayMCSD.Repositories.Interfaces;
using RandomPayMCSD.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();



string conecctionString = builder.Configuration.GetConnectionString("SqlRandom");
//builder.Services.AddDbContext<EmpleadosContext>
//    (options => options.UseSqlServer(conecctionString));
builder.Services.AddDbContext<RandomPayContext>
    (options => options.UseSqlServer(conecctionString));
//builder.Services.AddTransient<IRepositoryEmpleados,RepositoryEmpleadosSQLServer>();
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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
