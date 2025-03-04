namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class ClientsDebtRequest
    {
        public string DealerId { get; set; } = null!;
        public int DeliveryDay { get; set; }

    }
    public class ClientsDebtResponse
    {
        public List<ClientItem> Clients { get; set; } = [];

        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public decimal Debt { get; set; }
        }
    }
}
