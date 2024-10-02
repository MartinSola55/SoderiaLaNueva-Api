namespace SoderiaLaNueva_Api.Models.DAO.User

{
    public class GetFormDataResponse
    {
        public List<Item> Roles { get; set; } = [];

        public class Item
        {
            public string Id { get; set; } = null!;
            public string Name { get; set; } = null!;
        }
    }
}
