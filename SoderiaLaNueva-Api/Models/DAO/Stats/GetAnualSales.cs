namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetAnualSalesRequest
    {
        public int Year { get; set; }
    }

    public class GetAnualSalesResponse
    {
        public decimal Total { get; set; }
        public List<Item> Daily { get; set; } = [];

        public class Item
        {
            public string Period { get; set; } = null!;
            public decimal Total { get; set; }
        }
    }
}
