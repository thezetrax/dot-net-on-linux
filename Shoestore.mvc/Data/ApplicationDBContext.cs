using Microsoft.EntityFrameworkCore;

namespace shoestore.Data;

public class ApplicationDBContext : DbContext
{
    public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }
}