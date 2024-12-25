namespace SoderiaLaNueva_Api.Models.DAO.Dealer
{
    public class GetClientsDebtRequest
    {
        public string DealerId { get; set; } = null!;
    }
    public class GetClientsDebtResponse
    {
        public decimal TotalDebt { get; set; }
    }
}
