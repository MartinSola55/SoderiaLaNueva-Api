namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class GetOneRequest
    {
        public int Id { get; set; }
    }

    public class GetOneResponse
    {
        public int Id { get; set; }
        public string? DealerId { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Observations { get; set; }
        public decimal Debt { get; set; }
        public int? DeliveryDay { get; set; }
        public bool HasInvoice { get; set; }
        public string? InvoiceType { get; set; }
        public string? TaxCondition { get; set; }
        public string? CUIT { get; set; }

        public List<int> Products { get; set; } = [];
        public List<int> Subscriptions { get; set; } = [];
        public List<CartsTransfersHistoryItem> SalesHistory { get; set; } = [];
        public List<ProductHistoryItem> ProductHistory { get; set; } = [];

        public class CartsTransfersHistoryItem
        {
            public string Date { get; set; } = null!;
            public string Type { get; set; } = null!;
            public List<SaleItem> Items { get; set; } = [];
            public List<PaymentItem> Payments { get; set; } = [];
        }

        public class SaleItem
        {
            public string Name { get; set; } = null!;
            public int? Quantity { get; set; }
            public decimal? Price { get; set; }
        }

        public class PaymentItem
        {
            public decimal Amount { get; set; }
            public string Name { get; set; } = null!;
        }

        public class ProductHistoryItem
        {
            public string Name { get; set; } = null!;
            public string Type { get; set; } = null!;
            public int Quantity { get; set; }
            public string Date { get; set; } = null!;
        }
    }
}
