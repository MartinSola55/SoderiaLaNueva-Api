
namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class GetOneRequest
    {
        public int Id { get; set; }
    }

    public class GetOneResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public List<SubscriptionProductItem> SubscriptionProducts { get; set; } = [];
        public class SubscriptionProductItem
        {
            public string Id { get; set; } = null!;
            public int Quantity { get; set; }
        }
    }
}
