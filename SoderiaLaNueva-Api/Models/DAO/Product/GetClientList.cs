namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class GetClientListRequest
    {
        public int ProductId { get; set; }
    }

    public class GetClientListResponse
    {
        public List<ClientItem> Clients { get; set; } = [];

        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public AddressItem Address { get; set; } = null!;
            public string DealerName { get; set; } = null!;
            public int? DeliveryDay { get; set; }
        }

        public class AddressItem 
        {
            public string? HouseNumber { get; set; }
            public string? Road { get; set; }
        }
    }
}
