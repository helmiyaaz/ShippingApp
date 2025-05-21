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

        [HttpPost("ShipmentPlan/ImportExcelMonthly")]
        public async Task<IActionResult> ImportExcel([FromBody] ImportRequest request)
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
        public IActionResult GetShipmentSummary()
        {
            var today = DateTime.Today;
            var endOfWeek = today.AddDays(6 - (int)today.DayOfWeek);

            var groupedData = _context.ShipmentLogModels
            .Where(x => x.Plan_Ship_Date != null && x.Status != "shipped")
            .ToList() 
            .GroupBy(x => x.Week)
            .Select(g => new
            {
                Week = g.Key,
                Summary = g
                    .GroupBy(x => x.Delivery_Point)
                    .Select(dp =>
                    {
                        var today = DateTime.Today;
                        var endOfWeek = today.AddDays(6 - (int)today.DayOfWeek);

                        var todayMax = dp
                            .Where(x => x.Plan_Ship_Date == today)
                            .Max(x => (decimal?)x.Ctn_Number) ?? 0;

                        var next = dp
                            .Where(x => x.Plan_Ship_Date > today && x.Plan_Ship_Date <= endOfWeek)
                            .OrderBy(x => x.Plan_Ship_Date)
                            .FirstOrDefault();

                        return new
                        {
                            Destination = dp.Key,
                            TodayBox = todayMax,
                            NextDay = next?.Plan_Ship_Date?.ToString("dddd") ?? "-",
                            NextBox = next?.Ctn_Number ?? 0
                        };
                    })
                    .ToList()
            })
            .ToList();

            return Json(groupedData);
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
