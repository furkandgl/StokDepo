using System.ComponentModel.DataAnnotations;

namespace StokDepo.Models
{
    public class StockMovement
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public int QuantityChange { get; set; } // + giriş, - çıkış

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? Note { get; set; }

        // Hangi kullanıcı yaptı?
        public string? PerformedBy { get; set; }
    }
}
