using ECommerceSystem.Api.Data;
using ECommerceSystem.Api.Data.Mongo;
using ECommerceSystem.Shared.DTOs;
using ECommerceSystem.Shared.DTOs.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceSystem.Api.Controllers
{
    [Authorize(Roles = "Admin")]
    
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly WebDBContext _dbContext;
        private readonly MongoDbContext _mongoContext;

        // Inject context của SQL Server và MongoDB
        public AdminController(WebDBContext dbContext, MongoDbContext mongoContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mongoContext = mongoContext ?? throw new ArgumentNullException(nameof(mongoContext));
        }

        // API thống kê theo loại (revenue, orders, top-products)
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(string type, string? period = "")
        {
            try
            {
                // Kiểm tra đầu vào
                if (string.IsNullOrWhiteSpace(type))
                    return BadRequest(new { Error = "Tham số 'type' là bắt buộc." });

                type = type.ToLowerInvariant();
                period = period?.ToLowerInvariant();
                var result = new StatisticDTO();

                switch (type)
                {
                    // Thống kê doanh thu theo ngày / tháng / năm
                    case "revenue":
                        var query = _dbContext.Orders
                            .Where(o => o.Status != "Cancelled" && !o.IsDeleted);

                        // Gom nhóm theo thời gian
                        var grouped = (period ?? "day") switch
                        {
                            "day" => query.GroupBy(o => o.CreatedAt.Date),
                            "month" => query.GroupBy(o => new DateTime(o.CreatedAt.Year, o.CreatedAt.Month, 1)),
                            "year" => query.GroupBy(o => new DateTime(o.CreatedAt.Year, 1, 1)),
                            _ => query.GroupBy(o => o.CreatedAt.Date)
                        };

                        // Tổng hợp kết quả
                        result.Revenue = await grouped
                            .Select(g => new { Date = g.Key, Value = g.Sum(o => o.Total) } as object)
                            .ToListAsync();
                        break;

                    // Thống kê số lượng đơn hàng theo trạng thái
                    case "orders":
                        result.OrderCount = await _dbContext.Orders
                            .GroupBy(o => o.Status)
                            .Select(g => new { Status = g.Key, Count = g.Count() })
                            .ToDictionaryAsync(g => g.Status, g => g.Count);
                        break;

                    // Thống kê 5 sản phẩm bán chạy nhất
                    case "top-products":
                        var orderItems = await _dbContext.OrderItems
                            .GroupBy(oi => oi.ProductId)
                            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(oi => oi.Quantity) })
                            .OrderByDescending(g => g.Quantity)
                            .Take(5)
                            .ToListAsync();

                        var productDict = await _dbContext.Products
                            .Where(p => !p.IsDeleted)
                            .Select(p => new { p.Id, p.Name })
                            .ToDictionaryAsync(p => p.Id, p => p.Name);

                        result.TopProducts = orderItems
                            .Where(oi => productDict.ContainsKey(oi.ProductId))
                            .Select(oi => new
                            {
                                Id = oi.ProductId,
                                Name = productDict[oi.ProductId],
                                Quantity = oi.Quantity
                            } as object)
                            .ToList();
                        break;

                    // Trường hợp loại không hợp lệ
                    default:
                        return BadRequest(new { Error = "Tham số 'type' không hợp lệ." });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả lỗi chi tiết nếu có exception
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Detail = ex.InnerException?.Message,
                    Stack = ex.StackTrace
                });
            }
        }

        // API kiểm tra tồn kho thấp (<=10)
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory()
        {
            try
            {
                var lowStockProducts = await _dbContext.Products
                    .Where(p => p.Stock > 0 && p.Stock <= 10 && !p.IsDeleted)
                    .Select(p => new { p.Id, p.Name, p.Stock } as object)
                    .ToListAsync();

                return Ok(new { LowStock = lowStockProducts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        // API thống kê hành vi người dùng từ MongoDB logs
        [HttpGet("user-activity")]
        public async Task<IActionResult> GetUserActivity()
        {
            try
            {
                // Lấy toàn bộ logs từ Mongo
                var logs = await _mongoContext.Logs.Find(_ => true).ToListAsync();

                // Gom nhóm theo endpoint để đếm số lượt gọi
                var activities = logs
                    .GroupBy(l => l.Endpoint)
                    .Select(g => new { Action = g.Key, Count = g.Count() } as object)
                    .ToList();

                return Ok(new { Activities = activities });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
