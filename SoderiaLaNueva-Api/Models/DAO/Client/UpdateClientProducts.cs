namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class UpdateClientProductsRequest
    {
        public int ClientId { get; set; }
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }

    public class UpdateClientProductsResponse
    {
    }
}
