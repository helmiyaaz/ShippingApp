using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FILog.Models
{
    [Table("DMSFGStock")]
    public class FGStockModel
    {
        [Key]

        public int id { get; set; }
        public string? Part_Number { get; set; }
        public string? Description { get; set; }
        public string? Order_Number { get; set; }
        public string? Batch_Number { get; set; }
        public decimal? Quantity { get; set; }
        public string? Field1 { get; set; }
        public string? Field2 { get; set; }
        public string? Field3 { get; set; }

    }
}
