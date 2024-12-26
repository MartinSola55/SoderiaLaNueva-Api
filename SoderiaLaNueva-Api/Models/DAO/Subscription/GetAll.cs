namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class GetAllRequest : GenericGetAllRequest
    {
        public string? Name { get; set; }
    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<Item> Subscriptions { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public List<SubscriptionProductItem> SubscriptionProductItems { get; set; } = [];
        }

        public class SubscriptionProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public int Quantity { get; set; }
        }
    }
}