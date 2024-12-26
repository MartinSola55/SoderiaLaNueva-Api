namespace SoderiaLaNueva_Api.Models.DAO.Subscription

{
    public class GetFormDataResponse
    {
        public List<Item> Products { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
        }
    }
}
