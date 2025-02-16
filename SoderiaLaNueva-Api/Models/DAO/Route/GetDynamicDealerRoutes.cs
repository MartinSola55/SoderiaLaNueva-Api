namespace SoderiaLaNueva_Api.Models.DAO.Route

{
    public class GetDynamicDealerRoutesRequest
    {
        public int DeliveryDay { get; set; }
    }

    public class GetDynamicDealerRoutesResponse
    {
        public List<RouteItem> Routes { get; set; } = [];

        public class RouteItem
        {
            public int Id { get; set; }
            public string Dealer { get; set; } = null!;
            public int TotalCarts { get; set; }
            public int CompletedCarts { get; set; }
            public decimal TotalCollected { get; set; }
            public string CreatedAt { get; set; } = null!;
        }
    }
}
