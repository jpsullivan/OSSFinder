using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSSFinder.Infrastructure.Attributes;

namespace OSSFinder.Controllers
{
    public partial class HomeController : Controller
    {
        [Route("")]
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}
