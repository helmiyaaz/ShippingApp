using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FILog.Models
{
    [Table("eLOGMaterialMaster")]
    public class eLogMaterialModel
    {
        [Key]

        public int id { get; set; }
        public string? OrderNumber {  get; set; }
        public string? PartNumber { get; set; }
        public string? Description { get; set; }
        public double? Quantity { get; set; }
        public string? BatchNumber { get; set; }
        public string? Order_Status { get; set; }
        public string? Current_OP { get; set; }
        public string? Current_WC { get; set; }
    }
}
