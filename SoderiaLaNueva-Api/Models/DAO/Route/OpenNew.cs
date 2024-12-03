namespace SoderiaLaNueva_Api.Models.DAO.Route
{
    public class OpenNewRequest
    {
        public int RouteId { get; set; }
    }

    public class OpenNewResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
    }
}
