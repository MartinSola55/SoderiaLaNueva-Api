namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class GetClientProductsRequest
    {
        public int Id { get; set; }
    }

    public class GetClientProductsResponse
    {
        public List<ProductItem> Products { get; set; } = [];
        public class ProductItem
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
        }
    }
}
