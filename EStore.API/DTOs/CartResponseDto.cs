namespace EStore.API.DTOs
{
    public class CartResponseDto
    {
        public List<CartItemResponseDto> Items { get; set; }
        public decimal TotalAmount { get; set; }

    }
}
