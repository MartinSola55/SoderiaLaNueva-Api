namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class NonVisitedClientsRequest
    {
        public string DealerId { get; set; } = null!;
        public DateTime DateFrom { get; set; } = DateTime.UtcNow;
        public DateTime DateTo { get; set; } = DateTime.UtcNow;
    }

    public class NonVisitedClientsResponse
    {
        public List<ClientItem> Clients { get; set; } = [];
        public int Total { get; set; }
        public int NonVisited { get; set; }

        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public string Address { get; set; } = null!;
        }
    }
}
