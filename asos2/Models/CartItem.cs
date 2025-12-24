using System.ComponentModel.DataAnnotations;

namespace IsisStore.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public string CartID { get; set; } 

        public int ProductID { get; set; }
        public Product Product { get; set; }

        public string SizeName { get; set; } 

        public int Quantity { get; set; }
    }
}