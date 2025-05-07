using System.Globalization;
using FILog.Data;
using FILog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WeeklyShipPlan.Models;

namespace AdminLTE4.Controllers
{
    public class PlanController : Controller
    {
        private readonly OpsProdDbContext _context;

        public PlanController(OpsProdDbContext context)
        {
            _context = context;
        }

        [HttpGet("ShipmentPlan/ReadData")]
        public async Task<IActionResult> GetAll()
        {
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Request.Query["start"].FirstOrDefault();
            var length = Request.Query["length"].FirstOrDefault();
            var searchValue = Request.Query["search[value]"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            var query = _context.MaterialMaster.AsQueryable();
             
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m => m.SO.Contains(searchValue) ||
                                         m.PO.Contains(searchValue) ||
                                         m.Part_Number.Contains(searchValue) ||
                                         m.Description.Contains(searchValue) ||
                                         m.Customer.Contains(searchValue));
            }

            int recordsTotal = await _context.MaterialMaster.CountAsync(); 
            int recordsFiltered = await query.CountAsync(); 

            var data = await query.Skip(skip).Take(pageSize).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,  
                recordsFiltered = recordsFiltered,
                data = data
            });
        }


        [HttpPost("ShipmentPlan/CreateData")]
        public async Task<IActionResult> CreateData([FromBody] WeeklyShipPlanModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Data tidak valid" });
            }

            _context.MaterialMaster.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { message = "Data berhasil ditambahkan", data = model });
        }

        [HttpPut("ShipmentPlan/Update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeeklyShipPlanModel updatedData)
        {
            if (updatedData == null || id != updatedData.id)
                return BadRequest(new { message = "Data tidak valid" });

            var existingData = await _context.MaterialMaster.FindAsync(id);
            if (existingData == null)
                return NotFound(new { message = "Data tidak ditemukan" });

         
            _context.Entry(existingData).CurrentValues.SetValues(updatedData);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil diperbarui" });
        }

        [HttpDelete("ShipmentPlan/Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var shipment = await _context.MaterialMaster.FindAsync(id);
            if (shipment == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            _context.MaterialMaster.Remove(shipment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data berhasil dihapus" });
        }

        [HttpGet("ShipmentPlan/GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var shipment = await _context.MaterialMaster.FindAsync(id);

            if (shipment == null)
                return NotFound(new { message = "Data tidak ditemukan" });

            return Ok(shipment);
        }

        [HttpPost("ShipmentPlan/PreviewExcel")]
        public async Task<IActionResult> PreviewExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File tidak valid.");

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.First();
            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;

            var headers = new List<string>();
            for (int col = 1; col <= colCount; col++)
                headers.Add(worksheet.Cells[1, col].Text);

            var result = new List<Dictionary<string, string>>();
            for (int row = 2; row <= rowCount; row++)
            {
                var rowDict = new Dictionary<string, string>();
                for (int col = 1; col <= colCount; col++)
                {
                    rowDict[headers[col - 1]] = worksheet.Cells[row, col].Text;
                }
                result.Add(rowDict);
            }

            return Ok(result); 
        }

        [HttpPost("ShipmentPlan/ImportExcel")]
        public async Task<IActionResult> ImportExcel([FromBody] ImportRequest request)
        {
            var data = request.Data;
            var overwrite = request.Overwrite;
            var newData = new List<WeeklyShipPlanModel>();

            foreach (var row in data)
            {
                try
                {
                    var model = new WeeklyShipPlanModel
                    {
                        SO = row.GetValueOrDefault("SO"),
                        SO_Line = row.GetValueOrDefault("SO Line"),
                        PO = row.GetValueOrDefault("PO"),
                        PO_Line = row.GetValueOrDefault("PO Line"),
                        Part_Number = row.GetValueOrDefault("Part Number"),
                        Description = row.GetValueOrDefault("Description"),
                        Qty = TryParseDecimal(row.GetValueOrDefault("Qty")),
                        Customer = row.GetValueOrDefault("Customer"),
                        Delivery_Point = row.GetValueOrDefault("Delivery Point"),
                        SSD = TryParseDateTime(row.GetValueOrDefault("SSD")),
                        LSD = TryParseDateTime(row.GetValueOrDefault("LSD")),
                        CRD = TryParseDateTime(row.GetValueOrDefault("CRD")),
                        PSD = TryParseDateTime(row.GetValueOrDefault("PSD")),
                        Week = row.GetValueOrDefault("Week"),
                        COS = TryParseDecimal(row.GetValueOrDefault("COS")),
                        Ttl_COS = TryParseDecimal(row.GetValueOrDefault("Ttl_COS")),
                        Mode = row.GetValueOrDefault("Mode"),
                        Drawing_Rev = row.GetValueOrDefault("Drawing Rev"),
                        Remarks = row.GetValueOrDefault("Remarks"),
                        PO_Type = row.GetValueOrDefault("PO Type")
                    };

                    newData.Add(model);
                }
                catch
                {
                    continue;
                }
            }

            if (overwrite)
            {
                var allOldData = _context.MaterialMaster.ToList();
                _context.MaterialMaster.RemoveRange(allOldData);
            }

            _context.MaterialMaster.AddRange(newData);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Import berhasil", count = newData.Count });
        }

        [HttpGet("ShipmentPlan/ExportExcel")]
        public async Task<ActionResult> ExportExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = await _context.MaterialMaster.ToListAsync();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DMS WeeklyShipment");

                // buat headernya
                worksheet.Cells[1, 1].Value = "SO";
                worksheet.Cells[1, 2].Value = "SO Line";
                worksheet.Cells[1, 3].Value = "PO";
                worksheet.Cells[1, 4].Value = "PO Line";
                worksheet.Cells[1, 5].Value = "Part Number";
                worksheet.Cells[1, 6].Value = "Description";
                worksheet.Cells[1, 7].Value = "Qty";
                worksheet.Cells[1, 8].Value = "Customer";
                worksheet.Cells[1, 9].Value = "Delivery Point";
                worksheet.Cells[1, 10].Value = "SSD";
                worksheet.Cells[1, 11].Value = "LSD";
                worksheet.Cells[1, 12].Value = "CRD";
                worksheet.Cells[1, 13].Value = "PSD";
                worksheet.Cells[1, 14].Value = "Week";
                worksheet.Cells[1, 15].Value = "COS";
                worksheet.Cells[1, 16].Value = "Ttl_COS";
                worksheet.Cells[1, 17].Value = "Mode";
                worksheet.Cells[1, 18].Value = "Drawing Rev";
                worksheet.Cells[1, 19].Value = "Remarks";
                worksheet.Cells[1, 20].Value = "PO Type";

                // Data
                for (int i = 0; i < data.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = data[i].SO;
                    worksheet.Cells[row, 2].Value = data[i].SO_Line;
                    worksheet.Cells[row, 3].Value = data[i].PO;
                    worksheet.Cells[row, 4].Value = data[i].PO_Line;
                    worksheet.Cells[row, 5].Value = data[i].Part_Number;
                    worksheet.Cells[row, 6].Value = data[i].Description;
                    worksheet.Cells[row, 7].Value = data[i].Qty;
                    worksheet.Cells[row, 8].Value = data[i].Customer;
                    worksheet.Cells[row, 9].Value = data[i].Delivery_Point;
                    worksheet.Cells[row, 10].Value = data[i].SSD;
                    worksheet.Cells[row, 11].Value = data[i].LSD;
                    worksheet.Cells[row, 12].Value = data[i].CRD;
                    worksheet.Cells[row, 13].Value = data[i].PSD;
                    worksheet.Cells[row, 14].Value = data[i].Week;
                    worksheet.Cells[row, 15].Value = data[i].COS;
                    worksheet.Cells[row, 16].Value = data[i].Ttl_COS;
                    worksheet.Cells[row, 17].Value = data[i].Mode;
                    worksheet.Cells[row, 18].Value = data[i].Drawing_Rev;
                    worksheet.Cells[row, 19].Value = data[i].Remarks;
                    worksheet.Cells[row, 20].Value = data[i].PO_Type;
                }

                var stream = new MemoryStream(package.GetAsByteArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DMS_WeeklyShipmentPlan.xlsx");
            }
        }

        private decimal? TryParseDecimal(string? input)
        {
            if (decimal.TryParse(input, out var result))
                return result;
            return null;
        }

        private DateTime? TryParseDateTime(string value)
        {
            if (DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            if (DateTime.TryParseExact(value, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            if (DateTime.TryParse(value, out result))
                return result;

            return null;
        }



        public IActionResult IndexPreparedShipment()
        {
            ViewBag.ActiveTab = "Prepared";
            return View();
        }

        public IActionResult IndexShipmentPlan()
        {
            ViewBag.ActiveTab = "ShipmentPlan";
            return View();
        }

        public IActionResult indexInPreparation()
        {
            ViewBag.ActiveTab = "InPreparation";
            return View();
        }

        public IActionResult IndexAnalyze()
        {
            ViewBag.ActiveTab = "Analyze";
            return View();
        }

        public IActionResult IndexPortalData()
        {
            ViewBag.ActiveTab = "PortalData";
            return View();
        }
    }
}
