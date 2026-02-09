namespace EStore.API.DTOs
{
    public class OrderItemResponseDto
    {
 
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal unitPrice { get; set; }

        public decimal subTotal {get; set;}
       

    }
}
