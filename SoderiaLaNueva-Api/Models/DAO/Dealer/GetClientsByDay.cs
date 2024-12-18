namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetClientsByDayRequest
    {
        public string DealerId { get; set; } = null!;
        public int DeliveryDay { get; set; }
    }
    public class GetClientsByDayResponse
    {
        public List<ClientItem> Clients { get; set; } = null!;
        public class ClientItem
        {
            public string Name { get; set; } = null!;
            public decimal Debt { get; set; }
        }
    }
}
