using Microsoft.AspNetCore.Identity;

namespace SoderiaLaNueva_Api.Models
{
    public class ApiUser : IdentityUser
    {
        public string RoleId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        public virtual IdentityRole Role { get; set; } = null!;
    }
}