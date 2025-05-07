using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FILog.Models
{
    [Table("DMSShipmentLog")]
    public class ShipmentLogModel
    {
        [Key]

        public int id_log { get; set; }
        public string? Doc_Number { get; set; }
        public string? SO { get; set; }
        public string? SO_Line { get; set; }
        public string? PO { get; set; }
        public string? PO_Line { get; set; }
        public string? Part_Number { get; set; }
        public string? Description { get; set; }
        public decimal? Qty { get; set; }
        public string? Batch_Number { get; set; }
        public string? Serial_Number { get; set; }
        public string? Customer { get; set; }
        public string? Delivery_Point { get; set; }
        public string? ID { get; set; }
        public DateTime? SSD { get; set; }
        public DateTime? LSD { get; set; }
        public DateTime? CRD { get; set; }
        public DateTime? Plan_Ship_Date { get; set; }
        public string? Week { get; set; }
        public DateTime? Act_Ship_Date { get; set; }
        public string? Status { get; set; }
        public decimal? COS { get; set; }
        public decimal? Ttl_COS { get; set; }
        public decimal? Price { get; set; }
        public decimal? Ttl_Price { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Ttl_Weight { get; set; }
        public decimal? Ctn_Number { get; set; }
        public string? Mode { get; set; }
        public string? DN { get; set; }
        public string? ASN { get; set; }
        public string? AWB { get; set; }
        public string? Ship_Number { get; set; }
        public string? Bill_Doc { get; set; }
        public string? Shipper { get; set; }
        public string? POD { get; set; }
        public string? Remarks { get; set; }
        public string? Drawing_Rev { get; set; }
        public string? PO_Rev { get; set; }
        public string? Concession { get; set; }
        public string? Production_Permit { get; set; }
        public string? KFR { get; set; }
        public string? Special_Process { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public string? CoC_By { get; set; }
        public string? PEB { get; set; }
        public DateTime? PEB_Date { get; set; }
        public string? Lot_Number { get; set; }
        public string? EmailNotification { get; set; }
        public DateTime? POD_Date { get; set; }
        public decimal? Dlv_LT { get; set; }
        public string? Batch_Mtl { get; set; }
        public string? LessKITE_Reason { get; set; }
        public string? PO_Type { get; set; }
        public decimal? Est_Dlv_Cost { get; set; }
        public decimal? dt_mfg { get; set; }

    }
}
