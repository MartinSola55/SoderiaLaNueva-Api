namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class CreateStaticRequest
    {
        public string DealerId { get; set; } = null!;
        public int DeliveryDay { get; set; }
        public int TypeId { get; set; }
    }

    public class CreateStaticResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
    }
}
