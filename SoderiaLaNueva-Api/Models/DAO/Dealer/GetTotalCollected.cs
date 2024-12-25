namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetTotalCollectedRequest
    {
        public string DealerId { get; set; } = null!;
        public DateTime Date { get; set; }
    }
    public class GetTotalCollectedResponse
    {
        public decimal Total { get; set; }
    }
}
