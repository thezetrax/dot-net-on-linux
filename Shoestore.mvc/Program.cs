using Microsoft.EntityFrameworkCore;
using Shoestore.mvc.Data;

namespace Shoestore.mvc;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        

        var connectionString = builder.Configuration.GetConnectionString("Shoestore");
        builder.Services.AddDbContext<ApplicationDBContext>(options => options.UseSqlServer(connectionString));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        // Add services to the container.
        builder.Services
            .AddControllersWithViews()
            .AddRazorOptions(options => {
                options.ViewLocationFormats.Add("/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Shared/{0}.cshtml");
            });

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