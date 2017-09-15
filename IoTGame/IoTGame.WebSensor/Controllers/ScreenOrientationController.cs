using System;
using System.IO;
using System.Web.Mvc;

namespace IoTGame.WebSensor.Controllers
{
    public class ScreenOrientationController : Controller
    {
        // GET: ScreenOrientation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details()
        {
            var req = Request.InputStream;
            req.Seek(0, System.IO.SeekOrigin.Begin);
            string details = new StreamReader(req).ReadToEnd();
            WriteDetails(details);
            return Json(new { success = true });
        }

        public class DetailsDTO
        {
            public string details { get; set; }
        }

        private void WriteDetails(string details)
        {
            var ipAddress = Request.UserHostAddress;
            if (ipAddress == "::1")
            {
                ipAddress = "localhost";
            }
            var sentTime = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

            var fileName = Request.MapPath("~/Details/" + ipAddress + "-" + sentTime + ".log");

            var dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            System.IO.File.WriteAllText(fileName, details);
        }
    }
}