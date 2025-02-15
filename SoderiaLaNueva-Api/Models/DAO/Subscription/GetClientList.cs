namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class GetClientListRequest
    {
        public int SubscriptionId { get; set; }
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
            public string NameNumber { get; set; } = null!;
        }

    }
}
