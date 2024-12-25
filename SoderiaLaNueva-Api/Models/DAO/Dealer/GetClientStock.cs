namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetClientStockRequest
    {
        public string DealerId { get; set; } = null!;
    }
    public class GetClientStockResponse
    {
        public List<ProductItem> Products { get; set; } = null!;
        public class ProductItem
        {
            public string Name { get; set; } = null!;
            public int Stock { get; set; }
        }
    }
}
