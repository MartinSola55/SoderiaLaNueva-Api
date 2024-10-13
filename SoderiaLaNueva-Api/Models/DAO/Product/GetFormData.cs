namespace SoderiaLaNueva_Api.Models.DAO.Product

{
    public class GetFormDataResponse
    {
        public List<Item> ProductTypes { get; set; } = [];

        public class Item
        {
            public int Id { get; set; }
            public string Type { get; set; } = null!;
        }
    }
}
