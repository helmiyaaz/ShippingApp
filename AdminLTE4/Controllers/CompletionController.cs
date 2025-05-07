using Microsoft.AspNetCore.Mvc;

namespace AdminLTE4.Controllers
{
    public class CompletionController : Controller
    {
        public IActionResult IndexInProgress()
        {
            ViewBag.ActiveTab = "Progress";
            return View();
        }

        public IActionResult IndexASN()
        {
            ViewBag.ActiveTab = "ASN";
            return View();
        }

        public IActionResult IndexPacking()
        {
            ViewBag.ActiveTab = "Packing";
            return View();
        }

        public IActionResult IndexAWB()
        {
            ViewBag.ActiveTab = "AWB";
            return View();
        }

        public IActionResult IndexJabil() {
            ViewBag.ActiveTab = "Jabil";
            return View();
        }

        public IActionResult EditData()
        {
            ViewBag.ActiveTab = "Edit Data";
            return View();
        }

        public IActionResult IndexCancelation()
        {
            ViewBag.ActiveTab = "Cancelation";
            return View();
        }

        public IActionResult IndexInvoice()
        {
            ViewBag.ActiveTab = "Invoice";
            return View();
        }

        public IActionResult CreateShip()
        {
            ViewBag.ActiveTab = "Create Ship";
            return View();
        }

        public IActionResult CTG()
        {
            return View();
        }

    }
}
