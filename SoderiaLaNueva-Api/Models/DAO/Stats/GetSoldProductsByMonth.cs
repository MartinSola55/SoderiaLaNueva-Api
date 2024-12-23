namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetSoldProductsByMonthRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class GetSoldProductsByMonthResponse
    {
        public List<Item> Products { get; set; } = [];

        public class Item
        {
            public string Name { get; set; } = null!;
            public int Quantity { get; set; }
        }
    }
}
