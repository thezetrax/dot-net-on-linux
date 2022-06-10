using Shoestore.mvc.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shoestore.migrations;
public class MigrationStartup {
    public MigrationStartup()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        Configuration = builder.Build();

        //.. for test
        Console.WriteLine(Configuration.GetConnectionString("Shoestore"));
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<ApplicationDBContext>(options =>
        {
            options.UseSqlServer(Configuration.GetConnectionString("Shoestore"));
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env){}
}
