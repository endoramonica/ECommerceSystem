﻿namespace ECommerceSystem.Shared.DTOs.Product
{ 
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

}

