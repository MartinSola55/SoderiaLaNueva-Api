namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class SoldProductsByRangeRequest
    {
        public string DealerId { get; set; } = null!;
        public DateTime DateFrom { get; set; } = DateTime.UtcNow;
        public DateTime DateTo { get; set; } = DateTime.UtcNow;
    }

    public class SoldProductsByRangeResponse
    {
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public string Name { get; set; } = null!;
            public int Amount { get; set; }
            public decimal Total { get; set; }
        }
    }
}
