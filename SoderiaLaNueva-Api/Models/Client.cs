using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        public string? DealerId { get; set; }
        public string Name { get; set; } = null!;
        public int AddressId { get; set; }
        public string Phone { get; set; } = null!;
        public string? Observations { get; set; }
        public decimal Debt { get; set; } = 0;
        public int? DeliveryDay { get; set; }
        public bool HasInvoice { get; set; } = false;
        public string? InvoiceType { get; set; }
        public string? TaxCondition { get; set; }
        public string? CUIT { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }


        // Navigation properties
        public virtual ApiUser Dealer { get; set; } = null!;
        public virtual Address Address { get; set; } = null!;
        public virtual List<ClientSubscription> Subscriptions { get; set; } = null!;
        public virtual List<SubscriptionRenewal> SubscriptionRenewals { get; set; } = null!;
        public virtual List<ClientProduct> Products { get; set; } = null!;
        public virtual List<Cart> Carts { get; set; } = null!;
        public virtual List<Transfer> Transfers { get; set; } = null!;
    }
}