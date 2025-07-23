using ECommerceSystem.GUI.Apis;
using ECommerceSystem.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // <-- Add this line
using Refit;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace ECommerceSystem.GUI.Controllers
{
    //  [Authorize(Roles = "Admin")] loi do dinh cnflic giua asp net va http
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]

    public class AdminController : Controller
    {
        private readonly IAdminApi _adminApi;

        public AdminController(IAdminApi adminApi)
        {
            _adminApi = adminApi;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var revenue = await _adminApi.GetStatisticsAsync("revenue", "month");
                var topProducts = await _adminApi.GetStatisticsAsync("top-products");
                var orders = await _adminApi.GetStatisticsAsync("orders");

                ViewBag.Revenue = revenue.Revenue;
                ViewBag.TopProducts = topProducts.TopProducts;
                ViewBag.OrderCount = orders.OrderCount;

                return View();
            }
            catch (ApiException ex)
            {
                var errorObj = await SafeReadErrorAsync(ex);
                ViewBag.Error = $"API Error: {ex.Message}, Content: {errorObj}";
                return View();
            }
        }

        public async Task<IActionResult> UserActivity()
        {
            try
            {
                var result = await _adminApi.GetUserActivityAsync();
                ViewBag.Activities = result.Activities;
                return View();
            }
            catch (ApiException ex)
            {
                var errorObj = await SafeReadErrorAsync(ex);
                ViewBag.Error = $"API Error: {ex.Message}, Content: {errorObj}";
                return View();
            }
        }

        private async Task<string> SafeReadErrorAsync(ApiException ex)
        {
            try
            {
                var errorDict = await ex.GetContentAsAsync<Dictionary<string, object>>();
                return errorDict.ContainsKey("Error")
                    ? errorDict["Error"]?.ToString()
                    : JsonSerializer.Serialize(errorDict);
            }
            catch
            {
                return ex.Content ?? "Unknown error content";
            }
        }
    }
}
