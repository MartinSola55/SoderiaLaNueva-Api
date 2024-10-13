using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class SubscriptionRenewalProduct
    {
        [Key]
        public int Id { get; set; }
        public int SubscriptionRenewalId { get; set; }
        public int ProductTypeId { get; set; }
        public int AvailableQuantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual SubscriptionRenewal SubscriptionRenewal { get; set; } = null!;
        public virtual ProductType ProductType { get; set; } = null!;
    }
}