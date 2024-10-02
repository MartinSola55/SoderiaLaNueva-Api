namespace SoderiaLaNueva_Api.Models.DAO.User
{
    public class UpdateRequest : CreateRequest
    {
        public string Id { get; set; } = null!;
    }

    public class UpdateResponse
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; } = null!;
        public string? PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
