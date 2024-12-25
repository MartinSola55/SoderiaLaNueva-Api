namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetMonthlySalesRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class GetMonthlySalesResponse
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
