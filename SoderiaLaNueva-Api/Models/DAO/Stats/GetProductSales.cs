namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetProductSalesRequest
    {
        public int ProductId { get; set; }
        public int Year { get; set; }
    }

    public class GetProductSalesResponse
    {
        public int ClientStock { get; set; }
        public decimal TotalSold { get; set; }
        public List<int> Sales { get; set; } = [];

    }
}
