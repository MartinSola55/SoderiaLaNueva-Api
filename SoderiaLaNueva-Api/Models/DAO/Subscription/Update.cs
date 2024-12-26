namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class UpdateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public List<SubscriptionProductItem> SubscriptionProducts { get; set; } = [];
        public class SubscriptionProductItem
        {
            public int ProductTypeId { get; set; }
            public int Quantity { get; set; }
        }
    }

    public class UpdateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public List<int> ProductTypesIds { get; set; } = [];
    }
}