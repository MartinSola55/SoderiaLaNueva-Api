namespace SoderiaLaNueva_Api.Models
{
    public class Token
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
