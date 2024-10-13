using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.Models;

namespace SoderiaLaNueva_Api.DAL.DB;

public class APIContext(DbContextOptions<APIContext> options) : IdentityDbContext<ApiUser>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<ApiUser>().ToTable("User");
        builder.Entity<IdentityRole>().ToTable("Role");
        builder.Ignore<IdentityUserRole<string>>();
        builder.Ignore<IdentityUserToken<string>>();
        builder.Ignore<IdentityUserClaim<string>>();
        builder.Ignore<IdentityUserLogin<string>>();
        builder.Ignore<IdentityRoleClaim<string>>();

        builder.Entity<ApiUser>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Product>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<ProductType>().HasQueryFilter(x => x.DeletedAt == null);
    }

    // Entities
    public DbSet<ApiUser> User { get; set; }
    public DbSet<IdentityRole> Role { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ProductType> ProductType { get; set; }

}
