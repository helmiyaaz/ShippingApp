using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FILog.Models
{
    [Table("DMSMasterShipPlan")]

    public class MasterShipPlanModel
    {
        [Key]
        public int id { get; set; }
        public string? Part_Number { get; set; }
        public string? Description { get; set; }
        public decimal? Qty { get; set; }
        public string? Customer { get; set; }
        public DateTime? PSD { get; set; }
        public decimal? COS { get; set; }
        public decimal? Ttl_COS { get; set; }


    }
}
