using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CoreTodosMVC.Models;
using CoreFlogger;

namespace CoreTodosMVC.Controllers
{
    public class HomeController : Controller
    {        
        [TrackUsage("ToDos", "Core MVC", "View Home")]
        public IActionResult Index()
        {
            return View();
        }

        [TrackUsage("ToDos", "Core MVC", "View Contact")]
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {            
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? 
                HttpContext.TraceIdentifier });
        }
    }
}
