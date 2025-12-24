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
        public string SizeName { get; set; } 

        public int StockQuantity { get; set; }
    }
}