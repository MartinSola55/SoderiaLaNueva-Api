namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetOneRequest
    {
        public string DealerId { get; set; } = null!;
    }
    public class GetOneResponse
    {
        public string? DealerName { get; set; } = null!;
        public int TotalCarts { get; set; }
        public int TotalCartsCompleted { get; set; }
        public int TotalCartsNotCompleted { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal TotalCollected { get; set; }
    }
}
