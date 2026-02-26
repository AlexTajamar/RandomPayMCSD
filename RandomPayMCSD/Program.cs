using Microsoft.EntityFrameworkCore;
using RandomPayMCSD.Data;
using RandomPayMCSD.Repositories;
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
builder.Services.AddScoped<RepositoryUsuarios>();
builder.Services.AddScoped<RepositoryActividades>();
builder.Services.AddScoped<RepositoryGastos>();
builder.Services.AddScoped<BalanceService>();

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
