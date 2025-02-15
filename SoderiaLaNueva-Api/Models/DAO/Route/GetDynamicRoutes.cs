namespace SoderiaLaNueva_Api.Models.DAO.Route

{
    public class GetDynamicRoutesRequest
    {
        public DateTime? Date { get; set; }
        public int? DeliveryDay { get; set; }
    }

    public class GetDynamicRoutesResponse
    {
        public List<RouteItem> Routes { get; set; } = [];

        public class RouteItem
        {
            public int Id { get; set; }
            public string Dealer { get; set; } = null!;
            public int TotalCarts { get; set; }
            public int CompletedCarts { get; set; }
            public decimal TotalCollected { get; set; }
            public string? CreatedAt { get; set; }
            public List<SoldProductItem> SoldProducts { get; set; } = [];

            public class SoldProductItem
            {
                public string Name { get; set; } = null!;
                public int Amount { get; set; }
            }

        }
    }
}
