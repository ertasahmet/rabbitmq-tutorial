using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UdemyRabbitMqWatermark.Web.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;

        // Publisher tarafında service'i DI ile alıyoruz.
        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(ProductImageCreatedEvent productImageCreatedEvent)
        {
            // Bir publish metodu oluşturuyoruz ve Parametre olarak da publish edeceğimiz eventi alıyoruz.
            // Bağlan diyoruz. 
            var channel = _rabbitMQClientService.Connect();

            // Eventi json'dan sitring'e çeviriyoruz, ve sonra byte dizisine çeviriyoruz.
            var bodyString = JsonSerializer.Serialize(productImageCreatedEvent);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);

            // Properties oluşturup mesajı kalıcı hale getiriyoruz ki kaybolmasın.
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Channel'dan publish yapıyoruz.
            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWatermark, basicProperties: properties, body: bodyByte);
        }

    }
}
