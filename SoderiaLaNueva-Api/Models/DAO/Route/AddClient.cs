namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class AddClientRequest
    {
        public int RouteId { get; set; }
        public int ClientId { get; set; }
        public List<ProductItem> Products { get; set; } = null!;
        public List<PaymentMethodItem> PaymentMethods { get; set; } = null!;
        public class ProductItem
        {
            public int ProductTypeId { get; set; }
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }

        }
        public class PaymentMethodItem
        {
            public int Id { get; set; }
            public decimal Amount { get; set; }

        }
    }
}
