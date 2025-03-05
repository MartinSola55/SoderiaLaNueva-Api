namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class UpdateClientSubscriptionsRequest
    {
        public int ClientId { get; set; }
        public List<int> SubscriptionIds { get; set; } = [];

    }
    public class UpdateClientSubscriptionsResponse
    {
        public List<ProductItem> Products { get; set; } = [];
        public class ProductItem
        {
            public int ProductId { get; set; }
            public int Stock { get; set; }
        }

    }
}
