namespace SoderiaLaNueva_Api.Models.DAO.Expense
{
    public class CreateRequest
    {
        public string DealerId { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }

    public class CreateResponse
    {
        public int Id { get; set; }
        public string DealerName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }
    }
}