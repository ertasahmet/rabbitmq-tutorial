using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UdemyRabbitMqWatermark.Web.Services
{
    public class RabbitMQClientService : IDisposable
    {
        // Bir RabbitMQClientService oluşturduk. ConnectionFactory, connection ve channel'ları tanımladık. 
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;

        // Sabitleri tanımladık.
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        private readonly ILogger<RabbitMQClientService> _logger;

        // DI'da connectionFactory doldurduk.
        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public IModel Connect()
        {
            // Bir connect metodu oluştrduk ve bağlantı sağlıyoruz.
            _connection = _connectionFactory.CreateConnection();

            // Bağlantı açıksa channel'ı dönüyoruz.
            if (_channel is { IsOpen: true})
            {
                return _channel;
            }

            // Açık değilse burada channel oluşturuyoruz.
            _channel = _connection.CreateModel();

            // Exchange'i ve queue'yu declare ediyoruz.
            _channel.ExchangeDeclare(ExchangeName, type: ExchangeType.Direct, true, false);
            _channel.QueueDeclare(QueueName, true, false, false, null);

            // Oluşturudğumuz queue'yu exchange'e bind ediyoruz.
            _channel.QueueBind(QueueName, ExchangeName, RoutingWatermark, null);

            // Log ile bilgi veriyoruz.
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu.");

            return _channel;
        }

        // Dispose metodunda channel'ı ve connection'ı kapatıyoruz ki memory yemesin.
        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu.");
        }
    }
}
