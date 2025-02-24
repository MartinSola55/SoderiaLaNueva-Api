using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual ProductType Type { get; set; } = null!;
    }
}