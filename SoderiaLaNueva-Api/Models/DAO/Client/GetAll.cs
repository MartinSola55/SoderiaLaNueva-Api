namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public string? Name { get; set; } = null!;
        public int? Id { get; set; }
        public List<string>? DealerIds { get; set; } = null!;
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Clients { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string Address { get; set; } = null!;
            public string Phone{ get; set; } = null!;
            public decimal Debt { get; set; }
            public string DealerName { get; set; } = null!;
            public int? DeliveryDay { get; set; }
        }
    }
}