﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.Shared.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public int CategoryId { get; set; }
        public float Rating { get; set; }
        public bool IsPromoted { get; set; }
        public string QrCode { get; set; }
    }
}

