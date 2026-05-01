using EStore.API.Data;
using EStore.API.DTOs;
using EStore.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;
        public CartController(AppDbContext context)
        {
            _context = context;

        }
        //add to cart
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            // Implementation for adding product to cart

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity });
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Item added to cart" });
        }
        //get cart items
        [HttpGet]
        public async Task<IActionResult> GetCartItems()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return Ok(new CartResponseDto
                {
                    Items = new List<CartItemResponseDto>(),
                    TotalAmount = 0
                });

            }

            var cartItems = cart.Items.Select(i => new CartItemResponseDto
            {
                ProductName = i.Product.Name,
                Quantity = i.Quantity,
                UnitPrice = i.Product.Price,
                SubTotal = i.Quantity * i.Product.Price

            }).ToList();

            var totalAmount = cartItems.Sum(i => i.SubTotal);

            return Ok(new CartResponseDto
            {
                Items = cartItems,
                TotalAmount = totalAmount
            });
        }
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _context.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                return NotFound("Item not found in cart");
            }
            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item removed sucessfully" });
        }
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any())
            {
                return BadRequest("Cart is empty");

            }
            var order=new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Items =new List<OrderItem>()

            };
            decimal totalAmount = 0;
            foreach (var item in cart.Items)
            {
                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price

                };
                totalAmount += item.Quantity * item.Product.Price;
                order.Items.Add(orderItem);

            }
            order.TotalAmount = totalAmount;
            _context.Orders.Add(order);
            //Clear Cart
            _context.CartItems.RemoveRange(cart.Items);
             await _context.SaveChangesAsync();

          
            return Ok(new
            {
                message= "order placed successfully",orderid=order.Id
            });


        }

    }
}