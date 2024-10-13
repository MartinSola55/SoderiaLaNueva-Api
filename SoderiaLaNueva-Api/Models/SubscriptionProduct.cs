using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class SubscriptionProduct
    {
        [Key]
        public int Id { get; set; }
        public int SubscriptionId { get; set; }
        public int ProductTypeId { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Subscription Subscription { get; set; } = null!;
        public virtual ProductType ProductType { get; set; } = null!;
    }
}