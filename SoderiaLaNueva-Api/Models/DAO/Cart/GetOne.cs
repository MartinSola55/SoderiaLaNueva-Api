
using static SoderiaLaNueva_Api.Models.DAO.Route.GetDynamicRouteResponse.CartItem;

namespace SoderiaLaNueva_Api.Models.DAO.Cart
{
    public class GetOneRequest
    {
        public int Id { get; set; }
    }

    public class GetOneResponse
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int DeliveryDay { get; set; }
        public string Dealer { get; set; } = null!;
        public string Client { get; set; } = null!;
        public List<ProductItem> Products { get; set; } = [];
        public List<ClientProductItem> ClientProducts { get; set; } = [];
        public List<SubscriptionProductItem> SubscriptionProducts { get; set; } = [];
        public List<PaymentMethodItem> PaymentMethods { get; set; } = [];

        public class ProductItem
        {
            public int Id { get; set; }
            public int ProductTypeId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }
            public int SubscriptionQuantity { get; set; }
        }
        public class ClientProductItem
        {
            public int ProductId { get; set; }
            public int ProductTypeId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }

        public class SubscriptionProductItem
        {
            public int TypeId { get; set; }
            public string Name { get; set; } = null!;
            public int Available { get; set; }
        }

        public class PaymentMethodItem
        {
            public int Id { get; set; }
            public int PaymentMethodId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Amount { get; set; }
        }
    }
}
