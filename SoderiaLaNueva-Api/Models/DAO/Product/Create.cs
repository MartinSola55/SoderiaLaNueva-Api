namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class CreateRequest
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
    }

    public class CreateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
    }
}
