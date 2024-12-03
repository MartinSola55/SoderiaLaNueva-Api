namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class GetAllStaticRequest : GenericGetAllRequest
    {
        public int DeliveryDay { get; set; }
    }

    public class GetAllStaticResponse : GenericGetAllResponse
    {
        public List<Item> Routes { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Dealer { get; set; } = null!;
            public int TotalCarts { get; set; }
        }
    }
}