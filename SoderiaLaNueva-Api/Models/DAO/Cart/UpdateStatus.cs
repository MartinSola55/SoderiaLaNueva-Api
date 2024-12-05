
namespace SoderiaLaNueva_Api.Models.DAO.Cart
{
    public class UpdateStatusRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = null!;
    }
}
