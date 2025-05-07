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

    }
}
