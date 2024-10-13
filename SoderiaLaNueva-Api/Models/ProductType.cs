using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class ProductType
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public virtual List<Product> Products { get; set; } = null!;
    }
}