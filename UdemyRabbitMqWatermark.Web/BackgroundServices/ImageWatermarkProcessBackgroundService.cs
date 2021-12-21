using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UdemyRabbitMqWatermark.Web.Services;

namespace UdemyRabbitMqWatermark.Web.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        // Channel'ı ve rabbitMQService'i alıyoruz. Bu class'ı da backgroundService olarak tanımlıyoruz ki arkaplanda çalışsın.
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;

        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            // Service başladığında bağlantı kursun, mesajları 1'er şekilde alacağını söylüyor.
            _channel = _rabbitMQClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Async olarak consumer oluşturduk. ve consume olduk, queue'yu belirttik.
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer);

            // Mesaj geldiğinde çalışacak eventi tanımladık.
            consumer.Received += Consumer_Received;

            return Task.CompletedTask;
        }

        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                // Eventi gelen mesajdan aldık.
                var productImageCreatedEvent = JsonSerializer.Deserialize<ProductImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", productImageCreatedEvent.ImageName);

                var siteName = "AHMET ERTAŞ ABİ";

                // Burada fotoğrafa watermark ekleme işlemleri yaptık.
                using var img = Image.FromFile(path);
                using var grapghic = Graphics.FromImage(img);

                var font = new Font(FontFamily.GenericMonospace, 40, FontStyle.Bold, GraphicsUnit.Pixel);
                var textSize = grapghic.MeasureString(siteName, font);

                var color = Color.FromArgb(128, 255, 255, 255);
                var brush = new SolidBrush(color);

                var position = new Point(img.Width - ((int)textSize.Width + 30), img.Height - ((int)textSize.Height + 30));

                grapghic.DrawString(siteName, font, brush, position);

                img.Save("wwwroot/images/watermarks/" + productImageCreatedEvent.ImageName);

                img.Dispose();
                grapghic.Dispose();

                // En son tüm olay başarılı olunca rabbitMQ'ya mesajı başarılı işledik diyoe bilgi verdik.
                _channel.BasicAck(@event.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                // Hata varsa logladık.
                _logger.LogError(ex.Message);
            }

            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
