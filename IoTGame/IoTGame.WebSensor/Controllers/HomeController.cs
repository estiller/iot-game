using System.Web.Mvc;

namespace IoTGame.WebSensor.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}