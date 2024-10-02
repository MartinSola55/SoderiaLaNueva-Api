
namespace SoderiaLaNueva_Api.Models.DAO.User
{
    public class GetOneRequest
    {
        public string Id { get; set; } = null!;
    }

    public class GetOneResponse
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string? PhoneNumber { get; set; } 
        public string CreatedAt { get; set; } = null!;

    }
}
