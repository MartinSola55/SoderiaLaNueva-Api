namespace SoderiaLaNueva_Api.Models.DAO.Transfer
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public DateTime DateFrom { get; set; } = DateTime.UtcNow;
        public DateTime DateTo { get; set; } = DateTime.UtcNow;
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Transfers { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string ClientName { get; set; } = null!;
            public string? DealerName { get; set; } = null!;
            public decimal Amount { get; set; }
            public string CreatedAt { get; set; } = null!;
            public string DeliveredDate { get; set; } = null!;
        }
    }
}