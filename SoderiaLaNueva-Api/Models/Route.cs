using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Route
    {
        [Key]
        public int Id { get; set; }
        public string DealerId { get; set; } = null!;
        public int DeliveryDay { get; set; }
        public bool IsStatic { get; set; } = false;
        public bool IsClosed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ApiUser Dealer { get; set; } = null!;
        public virtual List<Cart> Carts { get; set; } = null!;
    }
}