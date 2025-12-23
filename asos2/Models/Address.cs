using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IsisStore.Models
{
    public class Address
    {
        [Key]
        public int AddressID { get; set; }

        public int UserID { get; set; }

        public string? AddressTitle { get; set; } // e.g. "Home", "Work"

        public string? AddressLine1 { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public bool IsDefault { get; set; }
    }
}