using System.ComponentModel.DataAnnotations;

namespace SoderiaLaNueva_Api.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        public string? HouseNumber { get; set; }
        public string? Road { get; set; }
        public string? Neighbourhood { get; set; }
        public string? Suburb { get; set; }
        public string? CityDistrict { get; set; }
        public string? City { get; set; }
        public string? Town { get; set; }
        public string? Village { get; set; }
        public string? County { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Postcode { get; set; }
        public string Lat { get; set; } = null!;
        public string Lon { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

    }
}