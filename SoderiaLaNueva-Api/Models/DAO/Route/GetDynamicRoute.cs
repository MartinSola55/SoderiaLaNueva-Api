namespace SoderiaLaNueva_Api.Models.DAO.Route

{
    public class GetDynamicRouteRequest
    {
        public int Id { get; set; }
    }

    public class GetDynamicRouteResponse
    {
        public int Id { get; set; }
        public string Dealer { get; set; } = null!;
        public int DeliveryDay { get; set; }
        public decimal TransfersAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public bool IsClosed { get; set; }
        public List<ProductTypeItem> ProductTypes { get; set; } = [];
        public List<CartItem> Carts { get; set; } = [];

        public class ProductTypeItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
        }

        public class CartItem
        {
            public int Id { get; set; }
            public string Status { get; set; } = null!;
            public string CreatedAt { get; set; } = null!;
            public string UpdatedAt { get; set; } = null!;
            public ClientItem Client { get; set; } = null!;
            public List<ProductItem> Products { get; set; } = [];
            public List<PaymentItem> PaymentMethods { get; set; } = [];

            public class ClientItem
            {
                public int Id { get; set; }
                public string Name { get; set; } = null!;
                public decimal Debt { get; set; }
                public string Phone { get; set; } = null!;
                public AddressItem Address { get; set; } = null!;
                public string? Observations { get; set; } = null!;
                public List<ClientProductItem> Products { get; set; } = [];
                public List<ClientSubscriptionProductItem> SubscriptionProducts { get; set; } = [];
                public List<LastProductItem> LastProducts { get; set; } = [];
            }

            public class AddressItem
            {
                public string? HouseNumber { get; set; }
                public string? Road { get; set; }
                public string? Neighbourhood { get; set; }
                public string? Suburb { get; set; }
                public string? CityDistrict { get; set; }
                public string? City { get; set; }
                public string? Town { get; set; }
                public string? Village { get; set; }
                public string? County { get; set; }
                public string? State { get; set; }
                public string? Country { get; set; }
                public string? Postcode { get; set; }
                public string Lat { get; set; } = null!;
                public string Lon { get; set; } = null!;

            }

            public class ClientProductItem
            {
                public int ProductId { get; set; }
                public int ProductTypeId { get; set; }
                public string Name { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
            }

            public class ClientSubscriptionProductItem
            {
                public int TypeId { get; set; }
                public string Name { get; set; } = null!;
                public int Available { get; set; }
            }

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

            public class PaymentItem
            {
                public string Name { get; set; } = null!;
                public decimal Amount { get; set; }
            }

            public class LastProductItem
            {
                public string Date { get; set; } = null!;
                public string Name { get; set; } = null!;
                public int SoldQuantity { get; set; }
                public int ReturnedQuantity { get; set; }

            }
        }
    }
}
