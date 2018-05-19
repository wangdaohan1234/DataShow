using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataShow.WebUI.Controllers
{
    public class CardController : Controller
    {
        // GET: Card
        public ActionResult Card()
        {
            return View();
        }

        public ActionResult StuMap()
        {
            return View();
        }

        public ActionResult PersonalMonthBill()
        {
            return View();
        }
    }
}