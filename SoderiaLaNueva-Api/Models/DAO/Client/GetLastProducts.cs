namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class GetLastProductsRequest
    {
        public int ClientId { get; set; }
    }

    public class GetLastProductsResponse
    {
        public List<ProductItem> LastProducts { get; set; } = [];

        public class ProductItem
        {
            public string Date { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }
        }
    }
}
