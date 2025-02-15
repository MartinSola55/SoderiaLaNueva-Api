
namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class GetClientsListRequest : GenericGetAllRequest
    {
        public int Id { get; set; }
    }

    public class GetClientsListResponse : GenericGetAllResponse
    {
        public List<ClientItem> Items { get; set; } = [];

        public class ClientItem
        {
            public int ClientId { get; set; }
            public string Name { get; set; } = null!;
            public AddressItem Address { get; set; } = null!;
        }

        public class AddressItem
        {
            public string NameNumber { get; set; } = null!;
        }
    }
}
