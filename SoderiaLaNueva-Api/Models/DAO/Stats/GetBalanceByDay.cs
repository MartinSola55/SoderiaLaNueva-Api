namespace SoderiaLaNueva_Api.Models.DAO.Stats
{
    public class GetBalanceByDayRequest
    {
        public DateTime Date { get; set; }
    }

    public class GetBalanceByDayResponse
    {
        public List<Item> Items { get; set; } = [];

        public class Item
        {
            public string Name { get; set; } = null!;
            public decimal Total { get; set; }
        }
    }
}
