
namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class GetOneRequest
    {
        public int Id { get; set; }
    }

    public class GetOneResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int TypeId { get; set; }
        public string CreatedAt { get; set; } = null!;

    }
}
