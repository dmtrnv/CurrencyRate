using CurrencyRate.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CurrencyRate.Controllers
{
    [Route("")]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("[action]")]
        public IActionResult Index()
        {
            return RedirectPermanent("~/Money/Rate");
        }

        [Route("[action]")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
