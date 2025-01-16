namespace SoderiaLaNueva_Api.Models.DAO.Cart
{
    public class ConfirmRequest
    {
        public int Id { get; set; }
        public List<ProductItem> Products { get; set; } = [];
        public List<SubscriptionProductItem> SubscriptionProducts { get; set; } = [];
        public List<PaymentMethodItem> PaymentMethods { get; set; } = []; 

        public class ProductItem
        {
            public int ProductTypeId { get; set; }
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }
        }
        public class SubscriptionProductItem
        {
            public int ProductTypeId { get; set; }
            public int Quantity { get; set; }
        }
        public class PaymentMethodItem
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }
        }
    }

    public class ConfirmResponse
    {
        public int Id { get; set; }
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public int ProductTypeId { get; set; }
            public int ProductId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }
            public int SubscriptionQuantity { get; set; }
        }
    }
}
