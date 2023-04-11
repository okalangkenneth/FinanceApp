using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "Sorry, the resource you requested could not be found.";
                    break;
                case 500:
                    ViewBag.ErrorMessage = "Sorry, something went wrong on the server.";
                    break;
                // Add more cases for other status codes if needed
                default:
                    ViewBag.ErrorMessage = "An unexpected error occurred.";
                    break;
            }
            return View("Error");
        }

    }
}
