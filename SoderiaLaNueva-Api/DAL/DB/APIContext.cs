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
        builder.Entity<Cart>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<CartPaymentMethod>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<CartProduct>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Address>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Client>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<ClientProduct>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<ClientSubscription>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Expense>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<PaymentMethod>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<ProductType>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Models.Route>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Subscription>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<SubscriptionProduct>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<SubscriptionRenewal>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<SubscriptionRenewalProduct>().HasQueryFilter(x => x.DeletedAt == null);
        builder.Entity<Transfer>().HasQueryFilter(x => x.DeletedAt == null);
    }

    // Entities
    public DbSet<ApiUser> User { get; set; }
    public DbSet<IdentityRole> Role { get; set; }
    public DbSet<Cart> Cart { get; set; }
    public DbSet<CartPaymentMethod> CartPaymentMethod { get; set; }
    public DbSet<CartProduct> CartProduct { get; set; }
    public DbSet<Client> Client { get; set; }
    public DbSet<Address> Address { get; set; }
    public DbSet<ClientProduct> ClientProduct { get; set; }
    public DbSet<ClientSubscription> ClientSubscription { get; set; }
    public DbSet<Expense> Expense { get; set; }
    public DbSet<PaymentMethod> PaymentMethod { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ProductType> ProductType { get; set; }
    public DbSet<Models.Route> Route { get; set; }
    public DbSet<Subscription> Subscription { get; set; }
    public DbSet<SubscriptionProduct> SubscriptionProduct { get; set; }
    public DbSet<SubscriptionRenewal> SubscriptionRenewal { get; set; }
    public DbSet<SubscriptionRenewalProduct> SubscriptionRenewalProduct { get; set; }
    public DbSet<Transfer> Transfer { get; set; }

}
