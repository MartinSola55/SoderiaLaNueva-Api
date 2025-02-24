using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoderiaLaNueva_Api.DAL.DB;
using SoderiaLaNueva_Api.Models;
using SoderiaLaNueva_Api.Models.Constants;

namespace SoderiaLaNueva_Api.DAL.Seeding
{
    public class Seeder(APIContext db, IConfiguration config, UserManager<ApiUser> userManager, RoleManager<IdentityRole> roleManager) : ISeeder
    {
        private readonly APIContext _db = db;
        private readonly IConfiguration _config = config;
        private readonly UserManager<ApiUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        public void Seed()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                    _db.Database.Migrate();

                if (_db.Roles.Any(x => x.Name == Roles.Admin))
                    return;

                // Create roles
                foreach (var role in Roles.GetRoles())
                    _roleManager.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();

                var roleId = _db.Roles.Where(r => r.Name == Roles.Admin).Select(r => r.Id).FirstOrDefault() ?? throw new Exception("No se ha encontrado el rol");

                // Creaet users
                ApiUser admin = new()
                {
                    UserName = _config["AdminEmail"],
                    Email = _config["AdminEmail"],
                    EmailConfirmed = true,
                    FullName = "Admin 1",
                    RoleId = roleId
                };

                _userManager.CreateAsync(admin, _config["AdminPassword"] ?? "Password1!").GetAwaiter().GetResult();

                SeedProductTypes();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region DB Seed
        private void SeedProductTypes()
        {
            _db.ProductType.AddRange(ProductTypes.GetProductTypes().Select(x => new ProductType() { Name = x }));

            _db.SaveChanges();
        }
        #endregion
    }
}