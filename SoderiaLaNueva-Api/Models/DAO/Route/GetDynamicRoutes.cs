namespace SoderiaLaNueva_Api.Models.DAO.Route

{
    public class GetDynamicRoutesRequest
    {
        public DateTime Date { get; set; }
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
            public List<ProductItem> SoldProducts { get; set; } = [];

            public class ProductItem
            {
                public string Name { get; set; } = null!;
                public int Quantity { get; set; }
            }
        }
    }
}
