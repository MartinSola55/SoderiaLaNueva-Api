namespace SoderiaLaNueva_Api.Models.DAO.Expense
{
    public class UpdateRequest
    {
        public int Id { get; set; }
        public string DealerId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class UpdateResponse
    {
        public int Id { get; set; }
        public string DealerId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CreatedAt { get; set; } = null!;
    }
}