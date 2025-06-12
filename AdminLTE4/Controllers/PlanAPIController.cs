using System.Globalization;
using FILog.Data;
using FILog.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using WeeklyShipPlan.Models;

namespace FILog.Controllers
{
    public class PlanAPIController : Controller
    {
        private readonly OpsProdDbContext _context;
        public PlanAPIController(OpsProdDbContext context)
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

            if (double.TryParse(value, out double oaDate))
            {
                try
                {
                    return DateTime.FromOADate(oaDate); // ini lebih aman daripada manual 1/1/1900 + days
                }
                catch
                {
                    // Jika angka di luar range valid Excel date
                    return null;
                }
            }

            if (DateTime.TryParse(value, out result))
                return result;

            return null;
        }



        [HttpPost("ShipmentPlan/ImportExcelMonthly")]
        public async Task<IActionResult> ImportExcelMonthly([FromBody] ImportRequest request)
        {
            var data = request.Data;
            var overwrite = request.Overwrite;
            var newData = new List<MasterShipPlanModel>();

            foreach (var row in data)
            {
                try
                {
                    var model = new MasterShipPlanModel
                    {

                        Part_Number = row.GetValueOrDefault("Part Number"),
                        Description = row.GetValueOrDefault("Description"),
                        Qty = TryParseDecimal(row.GetValueOrDefault("Qty")),
                        Customer = row.GetValueOrDefault("Customer"),
                        PSD = TryParseDateTime(row.GetValueOrDefault("PSD")),
                        COS = TryParseDecimal(row.GetValueOrDefault("COS")),
                        Ttl_COS = TryParseDecimal(row.GetValueOrDefault("Ttl_COS")),
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
                var allOldData = _context.MasterShipPlanModels.ToList();
                _context.MasterShipPlanModels.RemoveRange(allOldData);
            }

            _context.MasterShipPlanModels.AddRange(newData);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Import berhasil", count = newData.Count });
        }


        [HttpGet("ShipmentLog/ReadData")]
        public async Task<IActionResult> GetAllShipmentLog()
        {
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Request.Query["start"].FirstOrDefault();
            var length = Request.Query["length"].FirstOrDefault();
            var searchValue = Request.Query["search[value]"].FirstOrDefault();
            var customerFilter = Request.Query["customer"].FirstOrDefault();

            int pageSize = length != null ? Convert.ToInt32(length) : 10;
            int skip = start != null ? Convert.ToInt32(start) : 0;

            IQueryable<ShipmentLogModel> query;

            if (!string.IsNullOrEmpty(customerFilter) && customerFilter != "All Data")
            {
                query = _context.ShipmentLogModels
                                .Where(m => m.Customer == customerFilter);
            }
            else
            {
                query = _context.ShipmentLogModels
                                .Where(m => m.Status == "shipped");
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m => (m.SO ?? "").Contains(searchValue) ||
                                         (m.PO ?? "").Contains(searchValue) ||
                                         (m.Part_Number ?? "").Contains(searchValue) ||
                                         (m.Description ?? "").Contains(searchValue) ||
                                         (m.Customer ?? "").Contains(searchValue));
            }

            int recordsFiltered = await query.CountAsync();
            var data = await query.Skip(skip).Take(pageSize).ToListAsync();

            int recordsTotal = await _context.ShipmentLogModels
                                             .Where(m => m.Status == "shipped")
                                             .CountAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        [HttpGet("ShipmentLog/GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.ShipmentLogModels
                                          .Select(m => m.Customer)
                                          .Distinct()
                                          .ToListAsync();

            return Json(customers);
        }

        [HttpGet("ShipmentLog/SumWeights")]
        public async Task<IActionResult> GetWeightSums(string customer)
        {
            IQueryable<ShipmentLogModel> query;

            if (!string.IsNullOrEmpty(customer) && customer != "All Data")
            {
                query = _context.ShipmentLogModels
                                .Where(m => m.Customer == customer);
            }
            else
            {
                query = _context.ShipmentLogModels
                                .Where(m => m.Status == "shipped");
            }

            var totalWeight = await query.SumAsync(m => (decimal?)m.Weight) ?? 0;
            var totalTtlWeight = await query.SumAsync(m => (decimal?)m.Ttl_Weight) ?? 0;

            return Json(new
            {
                weight = totalWeight,
                ttlWeight = totalTtlWeight
            });
        }

        [HttpGet("ShipmentPlan/GetTtlCos")]
        public async Task<IActionResult> GetCosSums()
        {
            IQueryable<WeeklyShipPlanModel> query = _context.MaterialMaster;

            var totalCos = await query.SumAsync(m => (decimal?)m.Ttl_COS) ?? 0;
            var totalLine = await query.CountAsync();

            return Json(new
            { 
                totalCos = totalCos,
                totalLine = totalLine
            });
        }
        [HttpGet("ShipmentLog/GetDelvpoint")]
        public async Task<IActionResult> GetDeliveryPoint()
        {
            var points = await _context.MaterialMaster
                .Select(m => m.Delivery_Point)
                .Distinct()
                .ToListAsync();

            return Json(points);
        }

        [HttpGet("ShipmentPlan/ReadAnalyze")]
        public async Task<IActionResult> GetDataAnalyze()
        {
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Request.Query["start"].FirstOrDefault();
            var length = Request.Query["length"].FirstOrDefault();
            var searchValue = Request.Query["search[value]"].FirstOrDefault();

            var deliveryPoint = Request.Query["Delivery_Point"].FirstOrDefault(); // HARUS SESUAI!

            int pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            int skip = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);

            IQueryable<WeeklyShipPlanModel> query = _context.MaterialMaster;

            if (string.IsNullOrEmpty(deliveryPoint))
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<object>()
                });
            }

            query = query.Where(m => m.Delivery_Point == deliveryPoint);

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m =>
                    (m.Part_Number ?? "").Contains(searchValue) ||
                    (m.Description ?? "").Contains(searchValue));
            }

            int recordsFiltered = await query.CountAsync();
            var data = await query.Skip(skip).Take(pageSize).ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsFiltered,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        [HttpGet("Analyze/GetSummaryByPart")]
        public IActionResult GetSummaryByPart(string partNumber)
        {
            var planQty = _context.MaterialMaster
                .Where(x => x.Part_Number == partNumber)
                .Sum(x => x.Qty) ?? 0;

            var stockQty = _context.FgStockModels
                .Where(x => x.Part_Number == partNumber)
                .Sum(x => x.Quantity) ?? 0;

            var preparedQty = _context.ShipmentLogModels
                .Where(x => x.Part_Number == partNumber && x.Status != "shipped")
                .Sum(x => (int?)x.Qty) ?? 0;

            var openASN = _context.PortalModels
                .Where(x => x.Part_Number == partNumber && x.ASN == "Y")
                .Sum(x => (int?)x.Qty) ?? 0;

            var wipQty = _context.eLogMaterialModels
                .Where(x => x.PartNumber == partNumber && (x.BatchNumber != "" && x.BatchNumber != null))
                .Sum(x => (int?)x.Quantity) ?? 0;

            var wipList = _context.eLogMaterialModels
                .Where(x => x.PartNumber == partNumber)
                .Select(x => new {
                    x.PartNumber,
                    x.Description,
                    Quantity = x.Quantity ?? 0,
                    x.Current_OP,
                    x.Current_WC
                }).ToList();

            var materialList = _context.MaterialMaster
                .Where(x => x.Part_Number == partNumber)
                .OrderBy(x => x.LSD)
                .ToList();

            decimal accQty = 0;
            var summaryList = materialList.Select(x =>
            {
                accQty += x.Qty ?? 0;
                decimal balStock = stockQty - accQty - preparedQty;

                decimal balASN = 0;

                if (x.Delivery_Point != "Bedok" && x.Delivery_Point != "RLC")
                {
                    balASN = openASN - accQty;
                }

                return new
                {
                    x.SO,
                    SO_Line = x.SO_Line,
                    Part_Number = x.Part_Number,
                    x.Description,
                    Qty = x.Qty ?? 0,
                    LSD = x.LSD?.ToString("yyyy-MM-dd"),
                    PSD = x.PSD?.ToString("yyyy-MM-dd"),
                    AccQty = accQty,
                    BalStock = balStock,
                    BalASN = balASN
                };
            }).ToList();


            return Json(new
            {
                partNumber,
                planQty,
                stockQty,
                openASN,
                preparedQty,
                wipQty,
                summaryList,
                wipList
            });
        }

        [HttpGet("Portal/ReadAllData")]
        public async Task<IActionResult> GetPortalData()
        {
            var draw = Request.Query["draw"].FirstOrDefault();
            var start = Request.Query["start"].FirstOrDefault();
            var length = Request.Query["length"].FirstOrDefault();
            var searchValue = Request.Query["search[value]"].FirstOrDefault();

            var customer = Request.Query["Customer"].FirstOrDefault();
            var partNumber = Request.Query["PartNumber"].FirstOrDefault();
            var po = Request.Query["PO"].FirstOrDefault();

            int pageSize = string.IsNullOrEmpty(length) ? 10 : Convert.ToInt32(length);
            int skip = string.IsNullOrEmpty(start) ? 0 : Convert.ToInt32(start);

            IQueryable<PortalModel> query = _context.PortalModels;

            if (!string.IsNullOrEmpty(customer) && customer != "Pilih...")
            {
                query = query.Where(m => m.Customer.Contains(customer));
            }

            if (!string.IsNullOrEmpty(partNumber))
            {
                query = query.Where(m => (m.Part_Number ?? "").Contains(partNumber));
            }

            if (!string.IsNullOrEmpty(po))
            {
                query = query.Where(m => (m.PO ?? "").Contains(po));
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(m => (m.ASN ?? "").Contains(searchValue) ||
                                         (m.PO ?? "").Contains(searchValue) ||
                                         (m.Part_Number ?? "").Contains(searchValue) ||
                                         (m.Description ?? "").Contains(searchValue) ||
                                         (m.Customer ?? "").Contains(searchValue));
            }

            int recordsFiltered = await query.CountAsync();

            var data = await query
                .OrderBy(m => m.Commit_Date) // optional: urutkan by Commit_Date seperti di VB.NET
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return Json(new
            {
                draw = draw,
                recordsTotal = recordsFiltered,
                recordsFiltered = recordsFiltered,
                data = data
            });
        }

        [HttpGet("Portal/GetCustomer")]
        public async Task<IActionResult> GetCustomerportal()
        {
            var points = await _context.PortalModels
                .Select(m => m.Customer)
                .Distinct()
                .ToListAsync();

            return Json(points);
        }

        [HttpGet("Portal/ExportExcel")]
        public async Task<ActionResult> ExportExcelPortal()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var data = await _context.PortalModels.ToListAsync();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DMS PortalData");

                worksheet.Cells[1, 1].Value = "Part Number";
                worksheet.Cells[1, 2].Value = "PO";
                worksheet.Cells[1, 3].Value = "Sch Line";
                worksheet.Cells[1, 4].Value = "Qty";
                worksheet.Cells[1, 5].Value = "Customer";
                worksheet.Cells[1, 6].Value = "ASN";
                worksheet.Cells[1, 7].Value = "Ship Date";
                worksheet.Cells[1, 8].Value = "Commit Date";
                worksheet.Cells[1, 9].Value = "OTD Date";
                worksheet.Cells[1, 10].Value = "Dwg Rev";

                for (int i = 0; i < data.Count; i++)
                {
                    var row = i + 2;
                    worksheet.Cells[row, 1].Value = data[i].Part_Number;
                    worksheet.Cells[row, 2].Value = data[i].PO;
                    worksheet.Cells[row, 3].Value = data[i].Sch_Line;
                    worksheet.Cells[row, 4].Value = data[i].Qty;
                    worksheet.Cells[row, 5].Value = data[i].Customer;
                    worksheet.Cells[row, 6].Value = data[i].ASN;
                    worksheet.Cells[row, 7].Value = data[i].Req_Date;
                    worksheet.Cells[row, 8].Value = data[i].Commit_Date;
                    worksheet.Cells[row, 9].Value = data[i].OTD_Date;
                    worksheet.Cells[row, 10].Value = data[i].Remarks;


                }

                var stream = new MemoryStream(package.GetAsByteArray());

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DMS_PortalData.xlsx");
            }
        }

        [HttpGet("Shipment/Schedule")]
        public async Task<IActionResult> GetCurrentWeekShipmentSummary()
        {
            var today = DateTime.Today;
            var culture = CultureInfo.CurrentCulture;
            int currentWeek = culture.Calendar.GetWeekOfYear(today, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            var saturday = today.AddDays(DayOfWeek.Saturday - today.DayOfWeek);
            var deliveryPoints = await _context.ShipmentLogModels
                .Select(x => x.Delivery_Point)
                .Distinct()
                .ToListAsync();

            var result = new List<object>();

            foreach (var dp in deliveryPoints)
            {
                var todayMax = await _context.ShipmentLogModels
                    .Where(x => x.Delivery_Point == dp &&
                                x.Plan_Ship_Date == today &&
                                x.Status != "shipped")
                    .MaxAsync(x => (decimal?)x.Ctn_Number) ?? 0;

                var next = await _context.ShipmentLogModels
                    .Where(x => x.Delivery_Point == dp &&
                                x.Plan_Ship_Date > today &&
                                x.Plan_Ship_Date <= saturday &&
                                x.Status != "shipped")
                    .OrderBy(x => x.Plan_Ship_Date)
                    .FirstOrDefaultAsync();

                int? weekFromDB = null;

                int? weekFromDate = next?.Plan_Ship_Date != null
                    ? (int?)culture.Calendar.GetWeekOfYear(next.Plan_Ship_Date.Value, CalendarWeekRule.FirstDay, DayOfWeek.Sunday)
                    : null;

                if (int.TryParse(next?.Week, out int parsedWeek))
                {
                    weekFromDB = parsedWeek;
                }


                result.Add(new
                {
                    Destination = dp,
                    TodayBox = todayMax,
                    NextDay = next?.Plan_Ship_Date?.ToString("dddd") ?? "-",
                    NextBox = next?.Ctn_Number ?? 0,
                    IsWeekValid = weekFromDB == weekFromDate
                });
            }

            return Json(new
            {
                CurrentWeek = currentWeek,
                Summary = result
            });
        }
        [HttpGet("Shipment/GetWeekList")]
        public IActionResult GetWeekList()
        {
            var weeks = _context.ShipmentLogModels
                .Where(x => !string.IsNullOrEmpty(x.Week))
                .Select(x => x.Week)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            return Json(weeks);
        }

    }
}
