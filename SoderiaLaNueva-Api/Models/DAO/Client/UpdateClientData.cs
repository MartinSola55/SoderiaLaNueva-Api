namespace SoderiaLaNueva_Api.Models.DAO.Client
{
    public class UpdateClientDataRequest
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public AddressItem Address { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public decimal Debt { get; set; }
        public string? Observations { get; set; }
        public int? DeliveryDay { get; set; }
        public string? DealerId { get; set; }
        public bool HasInvoice { get; set; }
        public string? InvoiceType { get; set; }
        public string? TaxCondition { get; set; }
        public string? CUIT { get; set; }

        public class AddressItem
        {
            public string NameNumber { get; set; } = null!;
            public string State { get; set; } = null!;
            public string City { get; set; } = null!;
            public string Country { get; set; } = null!;
            public string Lat { get; set; } = null!;
            public string Lon { get; set; } = null!;
        }
    }

    public class UpdateClientDataResponse
    {
    }
}
