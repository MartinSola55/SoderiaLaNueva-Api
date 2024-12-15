using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Transfer
    {
        [Key]
        public int Id { get; set; }
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
        //public DateTime DeliveredDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual Client Client { get; set; } = null!;
    }
}