namespace SoderiaLaNueva_Api.Models.DAO.Subscription
{
    public class RenewByRouteRequest
    {
        public int RouteId { get; set; }
    }
    public class RenewByRouteResponse
    {
        public List<ClientDebt> ClientDebts { get; set; } = [];

        public class ClientDebt
        {
            public int ClienId { get; set; }
            public decimal Debt { get; set; }
        }
    }
}