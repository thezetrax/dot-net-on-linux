using System;
using Shoestore.mvc.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Shoestore.migrations;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Starting Migration...");
        var webhost = new WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseStartup<MigrationStartup>()
            .Build();

        using (var context = (ApplicationDBContext) (
                webhost.Services.GetService(typeof(ApplicationDBContext)) ??
                throw new InvalidOperationException("Context shouldn't be null")
            ))
        {
            context.Database.Migrate();
        }
        Console.WriteLine("Migration Done");
    }
}
