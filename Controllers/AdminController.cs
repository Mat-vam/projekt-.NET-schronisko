using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using projekt_.NET_schronisko.Data;
using projekt_.NET_schronisko.ViewModels;

namespace projekt_.NET_schronisko.Controllers
{
    [Authorize(Roles = "Pracownik")]
    public class AdminController : Controller
    {
        private readonly ShelterContext _shelterContext;
        public AdminController(ShelterContext shelterContext)
        {
            _shelterContext = shelterContext;
        }
        public IActionResult Index()
        {
            var userCount = _shelterContext.Users.Count();

            var model = new AdminDashboardViewModel
            {
                UserCount = userCount
            };

            return View(model);
        }
    }
}
