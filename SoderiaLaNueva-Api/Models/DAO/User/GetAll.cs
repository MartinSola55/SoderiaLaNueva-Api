namespace SoderiaLaNueva_Api.Models.DAO.User
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public List<string>? Roles { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Users { get; set; } = [];

        public class Item
        {
            public string Id { get; set; } = null!;
            public string FullName { get; set; } = null!;
            public string? Email { get; set; } = null!;
            public string? PhoneNumber { get; set; } = null!;
            public string Role { get; set; } = null!;
            public string CreatedAt { get; set; } = null!;
        }
    }
}