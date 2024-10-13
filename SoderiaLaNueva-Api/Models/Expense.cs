using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }
        public string DealerId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ApiUser Dealer { get; set; } = null!;
    }
}