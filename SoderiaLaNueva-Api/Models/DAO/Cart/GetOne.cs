
namespace SoderiaLaNueva_Api.Models.DAO.Cart
{
    public class GetOneRequest
    {
        public int Id { get; set; }
    }

    public class GetOneResponse
    {
        public int Id { get; set; }
        public int DeliveryDay { get; set; }
        public string Dealer { get; set; } = null!;
        public string Client { get; set; } = null!;
        public List<ProductItem> Products { get; set; } = [];
        public List<PaymentMethodItem> PaymentMethods { get; set; } = [];

        public class ProductItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }
        }

        public class PaymentMethodItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public decimal Amount { get; set; }
        }
    }
}
