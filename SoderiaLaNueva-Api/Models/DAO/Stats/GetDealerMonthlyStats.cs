namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetDealerMonthlyStatsRequest
    {
        public string DealerId { get; set; } = null!;
    }

    public class GetDealerMonthlyStatsResponse
    { 
        public decimal TotalAmount { get; set; }
        public int CompletedCarts { get; set; }
        public int IncompleteCarts { get; set; }
    }
}
