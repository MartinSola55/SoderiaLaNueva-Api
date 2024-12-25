namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetAllRequest : GenericGetAllRequest
    {

    }

    public class GetAllResponse : GenericGetAllResponse
    {
        public List<DealerItem> Dealers = [];

        public class DealerItem
        {
            public string Id { get; set; } = null!;
            public string? Name { get; set; }
            public string? Email { get; set; }
            public string CreatedAt { get; set; } = null!;
        }
    }
}
