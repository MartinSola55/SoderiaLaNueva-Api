namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class UpdateRequest : CreateRequest
    {
        public int Id { get; set; }
    }

    public class UpdateResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
    }
}
