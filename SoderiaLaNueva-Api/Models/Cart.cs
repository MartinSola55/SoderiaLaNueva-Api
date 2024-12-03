using SoderiaLaNueva_Api.Models.Constants;
using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        public int RouteId { get; set; }
        public int ClientId { get; set; }
        public string Status { get; set; } = CartStatuses.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Route Route { get; set; } = null!;
        public virtual Client Client { get; set; } = null!;
        public virtual List<CartProduct> Products { get; set; } = null!;
        public virtual List<CartPaymentMethod> PaymentMethods { get; set; } = null!;
    }
}