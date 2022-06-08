
using Microsoft.EntityFrameworkCore;
using Shoestore.mvc.Models;

namespace Shoestore.mvc.Data;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

    private DbSet<Shoe>? _shoe { get; set; }
    public DbSet<Shoe> Shoe {
        set => _shoe = value;
        get => _shoe ?? throw new InvalidOperationException("Uninitialized property" + nameof(Shoe));
    }
}