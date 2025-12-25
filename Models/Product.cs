using System;
using System.ComponentModel.DataAnnotations;

namespace StokDepo.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori zorunludur.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, 9999999, ErrorMessage = "Fiyat 0.01'den büyük olmalıdır.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur.")]
        [Range(0, 1000000, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır.")]
        public int Quantity { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public static readonly string[] Categories =
        {
            "Elektronik",
            "Gıda",
            "Kırtasiye",
            "Temizlik"
        };
    }
}
