using CarCareTracker.External.Interfaces;
using CarCareTracker.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CarCareTracker.Helper;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CarCareTracker.Logic;

namespace CarCareTracker.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVehicleDataAccess _dataAccess;
        private readonly IUserLogic _userLogic;
        private readonly IFileHelper _fileHelper;
        private readonly IConfigHelper _config;
        private readonly IExtraFieldDataAccess _extraFieldDataAccess;
        public HomeController(ILogger<HomeController> logger,
            IVehicleDataAccess dataAccess,
            IUserLogic userLogic,
            IConfigHelper configuration,
            IFileHelper fileHelper,
            IExtraFieldDataAccess extraFieldDataAccess)
        {
            _logger = logger;
            _dataAccess = dataAccess;
            _config = configuration;
            _userLogic = userLogic;
            _fileHelper = fileHelper;
            _extraFieldDataAccess = extraFieldDataAccess;
        }
        private int GetUserID()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public IActionResult Index(string tab = "garage")
        {
            return View(model: tab);
        }
        public IActionResult Garage()
        {
            var vehiclesStored = _dataAccess.GetVehicles();
            if (!User.IsInRole(nameof(UserData.IsRootUser)))
            {
                vehiclesStored = _userLogic.FilterUserVehicles(vehiclesStored, GetUserID());
            }
            return PartialView("_GarageDisplay", vehiclesStored);
        }
        public IActionResult Settings()
        {
            var userConfig = _config.GetUserConfig(User);
            var languages = _fileHelper.GetLanguages();
            var viewModel = new SettingsViewModel
            {
                UserConfig = userConfig,
                UILanguages = languages
            };
            return PartialView("_Settings", viewModel);
        }
        [HttpPost]
        public IActionResult WriteToSettings(UserConfig userConfig)
        {
            var result = _config.SaveUserConfig(User, userConfig);
            return Json(result);
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult GetExtraFieldsModal(int importMode = 0)
        {
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(importMode);
            return PartialView("_ExtraFields", recordExtraFields);
        }
        public IActionResult UpdateExtraFields(RecordExtraField record)
        {
            try
            {
                var result = _extraFieldDataAccess.SaveExtraFields(record);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            var recordExtraFields = _extraFieldDataAccess.GetExtraFieldsById(record.Id);
            return PartialView("_ExtraFields", recordExtraFields);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
