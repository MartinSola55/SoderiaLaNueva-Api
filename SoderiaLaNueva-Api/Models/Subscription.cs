using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual List<ClientSubscription> ClientSubscriptions { get; set; } = null!;
        public virtual List<SubscriptionProduct> Products { get; set; } = null!;
        public virtual List<SubscriptionRenewal> Renewals { get; set; } = null!;
    }
}