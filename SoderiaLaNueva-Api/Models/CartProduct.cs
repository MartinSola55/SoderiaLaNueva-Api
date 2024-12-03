using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class CartProduct
    {
        [Key]
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductTypeId { get; set; }
        public int SoldQuantity { get; set; } = 0;
        public int SubscriptionQuantity { get; set; } = 0;
        public int ReturnedQuantity { get; set; } = 0;
        public decimal SettedPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Cart Cart { get; set; } = null!;
        public virtual ProductType Type { get; set; } = null!;
    }
}