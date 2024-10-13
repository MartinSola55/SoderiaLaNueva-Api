using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class SubscriptionRenewal
    {
        [Key]
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public int ClientId { get; set; }
        public decimal SettedPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Subscription Subscription { get; set; } = null!;
        public virtual Client Client { get; set; } = null!;
        public virtual List<SubscriptionRenewalProduct> ProductTypes { get; set; } = null!;
    }
}