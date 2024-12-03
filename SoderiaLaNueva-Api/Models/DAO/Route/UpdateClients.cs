namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class UpdateClientsRequest
    {
        public int RouteId { get; set; }
        public List<int> Clients { get; set; } = [];
    }
}
