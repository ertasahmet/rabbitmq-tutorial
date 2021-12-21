using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UdemyRabbitMqWatermark.Web.Services
{
    // Eventi böyle tanımladık.
    public class ProductImageCreatedEvent
    {
        public string ImageName { get; set; }
    }
}
