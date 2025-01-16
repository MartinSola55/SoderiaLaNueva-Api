namespace SoderiaLaNueva_Api.Models.DAO.Product
{
    public class GetSoldProductsByDateRequest
    {
        public DateTime Date { get; set; }
    }

    public class GetSoldProductsByDateResponse
    {
        public List<ProductItem> Products { get; set; } = new List<ProductItem>();

        public class ProductItem
        {
            public string Name { get; set; } = null!;
            public int Sold { get; set; } = 0;
            public int Returned { get; set; } = 0;
        }
    }
}
