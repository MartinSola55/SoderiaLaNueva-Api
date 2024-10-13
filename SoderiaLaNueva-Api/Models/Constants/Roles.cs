
namespace SoderiaLaNueva_Api.Models.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Dealer = "Repartidor";

        public static bool Validate(string role)
        {
            return !string.IsNullOrEmpty(role) && typeof(Roles).GetFields().Any(f => f.GetValue(null)?.ToString() == role);
        }

        public static List<string> GetRoles()
        {
            var roles = new List<string>();

            foreach (var field in typeof(Roles).GetFields())
            {
                var value = field.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(value))
                    roles.Add(value);
            }

            return roles;
        }
    }
}
