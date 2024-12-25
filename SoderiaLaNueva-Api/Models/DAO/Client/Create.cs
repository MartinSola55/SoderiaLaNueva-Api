namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class CreateRequest
    {
        public string? DealerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Observations { get; set; }
        public int? DeliveryDay { get; set; }
        public bool HasInvoice { get; set; }
        public string? InvoiceType { get; set; }
        public string? TaxCondition { get; set; }
        public string? CUIT { get; set; }
        public List<ProductItem> Products { get; set; } = [];

        public class ProductItem
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
        }
    }

    public class CreateResponse
    {

    }
}
