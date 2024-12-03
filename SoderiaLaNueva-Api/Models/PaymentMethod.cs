using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class PaymentMethod
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual List<CartPaymentMethod> CartPaymentMethods { get; set; } = null!;
    }
}