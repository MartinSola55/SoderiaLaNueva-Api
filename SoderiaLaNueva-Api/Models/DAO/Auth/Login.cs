namespace SoderiaLaNueva_Api.Models.DAO.Auth
{
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = null!;
        public DateTime SessionExpiration { get; set; }
        public Item User { get; set; } = new();

        public class Item
        {
            public string Id { get; set; } = null!;
            public string Role { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string Email { get; set; } = null!;
        }
    }
}
