using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace UdemyRabbitMq.Publisher
{
    public enum LogNames
    {
        Critical = 1,
        Error = 2,
        Warning = 3,
        Info = 4
    }
    class ExchangeTypes
    {
        void handleDirectExchange()
        {
            // Önce bir factory açtık ve sunucudaki rabbitmq'ye bağlandık.
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            // Burada bir exchange oluşturduk ve ismini logs-direct koyduk. Exchange tipi de Direct'tir. Direct tip exchange kendine gelen mesajı direkt olarak ilgili belirtilen kuyruğa ekler ve route'lar ile çalışır. Örneğin critical log için bir mesaj geldi ve route'u da critical-route, biz o route'a dayanarak onu ilgili direct-queue-critical kuyruğuna ekliyoruz.
            channel.ExchangeDeclare("logs-direct2", durable: true, type: ExchangeType.Direct);


            Enum.GetNames(typeof(LogNames)).ToList().ForEach(x =>
            {
                // LogName'leri döndük ve routeKey oluşturduk. route-critical, route-info gibi route'lar oluştu ve aynı şekilde queue'lar da oluştu.
                var routeKey = $"route-{x}-{x}";
                var queueName = $"direct-queue-{x}-{x}";

                // Sonra bu isimlerle queue declare ettik.
                channel.QueueDeclare(queueName, true, false, false);

                // Sonra bu queue'ları routeKey'leri ile beraber exchange'e bind ettik.
                channel.QueueBind(queueName, "logs-direct2", routeKey);

            });


            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames log = (LogNames)new Random().Next(1, 5);

                // Sonra burda mesajları oluşturduk.
                string message = $"log-type: {log}";
                var messageBody = Encoding.UTF8.GetBytes(message);

                // Hangi mesajın hangi route'a gideceğini belirttik.
                var routeKey = $"route-{log}-{log}";

                // Burada publish yaparak hangi exchange'i publish edeceğimizi belirttik ve mesajımızı da gönderdik. RouteKey'i ile beraber ilgili exchange'e mesajı gönderdik. O da ilgili kuyruğa mesajı ileticek.
                channel.BasicPublish("logs-direct2", routeKey, null, messageBody);

                Console.WriteLine($"Mesaj gönderilmiştir: {message}");
            });

            Console.ReadLine();
        }

        void handleTopicExchange()
        {
            // Önce bir factory açtık ve sunucudaki rabbitmq'ye bağlandık.
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            // Burada topic exchange tanımladık. Topic exchange daha karışık route işlemlerinde işe yarıyor. Örneğin loglamada Error.Warning.Critical diye bir route'ımız var. Consumer tarafında bir kuyruk açıp sadece ortasında Warning olan mesajları al diyerek filtreleme belirtebiliriz.
            channel.ExchangeDeclare("logs-topic", durable: true, type: ExchangeType.Topic);

            Random rnd = new Random();

            Enumerable.Range(1, 50).ToList().ForEach(x =>
            {
                LogNames log1 = (LogNames)rnd.Next(1, 5);
                LogNames log2 = (LogNames)rnd.Next(1, 5);
                LogNames log3 = (LogNames)rnd.Next(1, 5);

                // Hangi mesajın hangi route'a gideceğini belirttik.
                var routeKey = $"{log1}.{log2}.{log3}";

                // Sonra burda mesajları oluşturduk.
                string message = $"log-type: {log1}-{log2}-{log3}";
                var messageBody = Encoding.UTF8.GetBytes(message);


                // Burada publish yaparak hangi exchange'i publish edeceğimizi belirttik ve mesajımızı da gönderdik. RouteKey'i ile beraber ilgili exchange'e mesajı gönderdik. O da ilgili kuyruğa mesajı ileticek.
                channel.BasicPublish("logs-topic", routeKey, null, messageBody);

                Console.WriteLine($"Log gönderilmiştir: {message}");
            });

            Console.ReadLine();
        }


    }
}
