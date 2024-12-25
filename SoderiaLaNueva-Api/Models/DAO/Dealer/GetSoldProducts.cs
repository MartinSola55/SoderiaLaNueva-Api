namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetSoldProductsRequest
    {
        public string DealerId { get; set; } = null!;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class GetSoldProductsResponse
    {
        public List<ProductItem> Products { get; set; } = null!;
        public class ProductItem
        {
            public string Name { get; set; } = null!;
            public int SoldQuantity { get; set; }
            public decimal TotalAmount { get; set; }
        }
    }
}
