using System.ComponentModel.DataAnnotations;

namespace IsisStore.Models
{
    public class ProductSize
    {
        [Key]
        public int SizeID { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; }

        [Required]
        public string SizeName { get; set; } // e.g., 'XS', 'L', 'EU 42'

        public int StockQuantity { get; set; }
    }
}