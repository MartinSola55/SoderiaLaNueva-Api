namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public List<int>? TypeIds { get; set; }
        public List<string> Statuses { get; set; } = ["active"];
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Products { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public string Type { get; set; } = null!;
            public string CreatedAt { get; set; } = null!;
            public bool IsActive { get; set; }
        }
    }
}