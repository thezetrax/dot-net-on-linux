using Microsoft.EntityFrameworkCore;

namespace Shoestore.mvc.Data;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

    public DbSet<Shoestore.mvc.Models.Shoe> Shoe { get; set; }
}