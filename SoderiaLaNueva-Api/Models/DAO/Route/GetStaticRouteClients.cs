
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
            public string Address { get; set; } = null!;
        }
    }
}
