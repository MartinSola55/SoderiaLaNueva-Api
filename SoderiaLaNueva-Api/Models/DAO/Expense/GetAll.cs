namespace SoderiaLaNueva_Api.Models.DAO.Expense
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public DateTime DateFrom { get; set; } = DateTime.UtcNow;
        public DateTime DateTo { get; set; } = DateTime.UtcNow;
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Expenses { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Description { get; set; } = null!;
            public decimal Amount { get; set; }
            public string CreatedAt { get; set; } = null!;
        }
    }
}