namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class ClientsStockRequest
    {
        public string DealerId { get; set; } = null!;
    }

    public class ClientsStockResponse
    {
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public string Name { get; set; } = null!;
            public int Stock { get; set; }
        }
    }
}
