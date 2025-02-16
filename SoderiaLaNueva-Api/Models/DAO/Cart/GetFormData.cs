namespace SoderiaLaNueva_Api.Models.DAO.Cart

{
    public class GetFormDataResponse
    {
        public List<string> CartStatuses { get; set; } = [];
        public List<string> CartTransfersTypes { get; set; } = [];
        public List<string> CartPaymentStatuses { get; set; } = [];
    }
}
