namespace SoderiaLaNueva_Api.Models.DAO.Transfer
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
        public string ClientName { get; set; } = null!;
        public string? DealerName { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}