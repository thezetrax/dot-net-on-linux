using Microsoft.EntityFrameworkCore;
using Shoestore.mvc.Data;

namespace Shoestore.mvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = @"Data Source=127.0.0.1,1433;Database=master;User=sa;Password=custom_password_123;";
        builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        } else {
            app.UseMigrationsEndPoint();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}