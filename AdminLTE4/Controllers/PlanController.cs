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
