namespace SoderiaLaNueva_Api.Models.DAO.Transfer
{
    public class CreateRequest
    {
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
    }

    public class CreateResponse
    {
        public int Id { get; set; }
        public string ClientName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string  Phone { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? DealerName { get; set; }
    }
}