
namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class GetStaticRouteRequest
    {
        public int Id { get; set; }
    }

    public class GetStaticRouteResponse
    {
        public int Id { get; set; }
        public string Dealer { get; set; } = null!;
        public int DeliveryDay { get; set; }
        public List<CartItem> Carts { get; set; } = [];

        public class CartItem
        {
            public int Id { get; set; }
            public int ClientId { get; set; }
            public string Name { get; set; } = null!;
            public decimal Debt { get; set; }
            public AddressItem Address { get; set; } = null!;
            public string Phone { get; set; } = null!;
            public string CreatedAt { get; set; } = null!;
            public string UpdatedAt { get; set; } = null!;

            public List<ProductItem> LastProducts { get; set; } = [];
        }

        public class AddressItem
        {
            public string NameNumber { get; set; } = null!;
        }

        public class ProductItem
        {
            public string Date { get; set; } = null!;
            public string Name { get; set; } = null!;
            public int SoldQuantity { get; set; }
            public int ReturnedQuantity { get; set; }

        }
    }
}
