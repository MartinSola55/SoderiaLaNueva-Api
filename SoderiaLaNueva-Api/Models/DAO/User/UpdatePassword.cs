namespace SoderiaLaNueva_Api.Models.DAO.User
{
    public class UpdatePasswordRequest
    {
        public string? Id { get; set; }
        public string Password { get; set; } = null!;
    }
}
