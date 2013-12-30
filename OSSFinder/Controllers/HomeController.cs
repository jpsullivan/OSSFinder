using System.Web.Mvc;
using OSSFinder.Infrastructure.Attributes;

namespace OSSFinder.Controllers
{
    public partial class HomeController : AppController
    {
        [Route("")]
        public virtual ActionResult Index()
        {
            return View();
        }
    }
}
