namespace SoderiaLaNueva_Api.Models.DAO.Client

{
    public class GetFormDataResponse
    {
        public List<string> InvoiceTypes { get; set; } = [];
        public List<string> TaxConditions { get; set; } = [];
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public int ProductId { get; set; } 
            public string Name { get; set; } = null!;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
        }
    }
}
