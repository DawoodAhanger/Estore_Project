using EStore.API.Data;
using EStore.API.DTOs;
using EStore.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductsController : ControllerBase
    {
        private  readonly AppDbContext _context;    
        public ProductsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task <IActionResult> GetAll()
        {
            var products = await _context.Products.Select(p => new ProductResponceDto
            {
                Id = p.Id,
                Name = p.Name,  
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                imageUrl = p.ImageUrl

            }).ToListAsync();
            return Ok (products);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product Not Found");
             }
            var result= new ProductResponceDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                imageUrl = product.ImageUrl
            };

            return Ok(result);
        }
        [Authorize(Roles ="Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateDto Dto)
        {

            var product= new Product
            {
                Name = Dto.Name,
                Description = Dto.Description,
                Price = Dto.Price,
                Stock = Dto.Stock,
                ImageUrl = Dto.imageUrl
            };
             _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return Ok( product);

        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        
        public async Task<IActionResult> Update(int id, ProductCreateDto Dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product Not Found");
            }
            product.Name = Dto.Name;
            product.Description = Dto.Description;
            product.Price = Dto.Price;
            product.Stock = Dto.Stock;
            product.ImageUrl = Dto.imageUrl;
            await _context.SaveChangesAsync();
            return Ok("Products updated sucessfully");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound("Product Not Found");
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok("Product deleted successfully");
        }
    }
}
