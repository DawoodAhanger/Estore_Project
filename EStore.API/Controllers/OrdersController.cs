using EStore.API.Data;
using EStore.API.DTOs;
using EStore.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (dto.Items == null || dto.Items.Count == 0)
            {
                return BadRequest("Order must have at least one item");
            }
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Items = new List<OrderItem>(),

            };
            decimal totalAmount = 0;
            foreach (var itemDto in dto.Items)
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with id {itemDto.ProductId} not found");
                }
                if (product.Stock < itemDto.Quantity)
                {
                    return BadRequest($"Insufficient stock for product {product.Name}");
                }
                product.Stock -= itemDto.Quantity;
                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    Price = product.Price
                };
                order.Items.Add(orderItem);



                totalAmount += itemDto.Quantity * product.Price;
            }
            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();




            return Ok(new { message = "order created sucessfully", orderId = order.Id });
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();

            }
            int userId = int.Parse(userIdClaim);
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Map orders to DTOs
            var response = orders.Select(o => new OrderResponseDto
            {
                OrderId = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Items = o.Items.Select(oi => new OrderItemResponseDto
                {

                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    unitPrice = oi.Price,
                    subTotal = oi.Quantity * oi.Price
                }).ToList()
            });
            return Ok(response);
        }

    }
}
    

    

