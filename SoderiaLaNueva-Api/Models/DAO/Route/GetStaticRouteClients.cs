
namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class GetStaticRouteClientsRequest
    {
        public int Id { get; set; }
    }

    public class GetStaticRouteClientsResponse
    {
        public int Id { get; set; }
        public string Dealer { get; set; } = null!;
        public int DeliveryDay { get; set; }
        public List<ClientItem> Clients { get; set; } = [];

        public class ClientItem
        {
            public int ClientId { get; set; }
            public string Name { get; set; } = null!;
            public AddressItem Address { get; set; } = null!;
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
    }
}
