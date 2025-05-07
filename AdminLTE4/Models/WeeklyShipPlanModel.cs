using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeeklyShipPlan.Models
{
    [Table("DMSWeeklyShipPlan")]
    public class WeeklyShipPlanModel
    {
        [Key]
        public int id { get; set; }
        public string? SO { get; set; }
        public string? SO_Line {  get; set; }
        public string? PO { get; set; }
        public string? PO_Line  { get; set;}
        public string? Part_Number { get; set; }
        public string? Description { get; set; }
        public decimal? Qty { get; set; }
        public string? Customer {  get; set; }
        public string? Delivery_Point { get; set; }
        public DateTime? SSD { get; set; }
        public DateTime? LSD { get; set; }
        public DateTime? CRD { get; set; }
        public DateTime? PSD { get; set; }
        public string? Week {  get; set; }
        public decimal? COS { get; set; }
        public decimal? Ttl_COS { get; set; }
        public string? Mode { get; set; }
        public string? Drawing_Rev { get; set; }
        public string? Remarks { get; set; }
        public string? PO_Type { get; set; }

    }
}
