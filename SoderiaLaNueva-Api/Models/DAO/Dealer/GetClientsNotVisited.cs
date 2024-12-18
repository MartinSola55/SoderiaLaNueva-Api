namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetClientsNotVisitedRequest
    {
        public string DealerId { get; set; } = null!;
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
    public class GetClientsNotVisitedResponse
    {
        public int TotalClients { get; set; }
        public List<ClientItem> Clients { get; set; } = null!;
        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public string Address { get; set; } = null!;
        }
    }
}
