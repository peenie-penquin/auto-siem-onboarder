using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoSiem.Areas.Identity.Data;
using AutoSiem.Models;


namespace AutoSiem.Controllers
{
    public class HomeController : Controller
    {

        public HomeController()
        {
            // This controller is not being used.
            // TicketController is now the Home controller where the main page is.
        }

        public IActionResult Index()
        {
            // redirect to ticket controller
            // make ticket controller the main page
            return RedirectToAction("Index", "Ticket");
        }
    }
}
