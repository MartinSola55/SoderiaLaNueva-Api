namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class UpdateClientSubscriptionsRequest
    {
        public int ClientId { get; set; }
        public List<int> SubscriptionIds { get; set; } = [];

    }
}
