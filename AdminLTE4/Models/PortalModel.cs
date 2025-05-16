using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FILog.Models
{
    [Table("DMSPortal")]
    public class PortalModel
    {
        [Key]
        public int id { get; set; }
        public string? Part_Number { get; set; }
        public string? Description { get; set; }
        public string? PO {  get; set; }
        public string? Sch_Line { get; set; }
        public int? Qty { get; set; }
        public string? Customer {  get; set; }
        public string? ASN { get; set; }
        public DateTime? Req_Date { get; set; }
        public DateTime? OTD_Date { get; set; }
        public DateTime? Commit_Date { get; set; }
        public string? Remarks { get; set; }
    }
}
