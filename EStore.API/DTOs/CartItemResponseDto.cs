namespace EStore.API.DTOs
{
    public class CartItemResponseDto
    {
        public string ProductName { get;set; }
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal SubTotal { get; set; }
    }
}
