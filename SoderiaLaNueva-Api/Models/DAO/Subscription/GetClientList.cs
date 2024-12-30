﻿namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class GetClientListRequest
    {
        public int SubscriptionId { get; set; }
    }

    public class GetClientListResponse
    {
        public List<ClientItem> Clients = [];

        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public string Address { get; set; } = null!;
            public string DealerName { get; set; } = null!;
            public int? DeliveryDay { get; set; }
        }
    }
}
