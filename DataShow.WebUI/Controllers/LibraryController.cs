using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataShow.WebUI.Controllers
{
    public class LibraryController : Controller
    {
        // GET: Library
        public ActionResult Library()
        {
            return View();
        }

        public ActionResult TSLYL()
        {
            return View();
        }

        public ActionResult RDTS()
        {
            return View();
        }

        public ActionResult BorrowDetail()
        {
            return View();
        }
    }
}