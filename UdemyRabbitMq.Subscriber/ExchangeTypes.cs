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
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);

            // Burada critical mesajları okuyan kuyruğu yazdık ve ona consume olduk. Ordan mesajları dinleyeceğiz. Başka bir consumer oluşturup ondan da warning olan log'ları dinleyip başka işlemler yapabiliriz.
            var queueName = "direct-queue-Critical-Critical";
            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Loglar dinleniyor...");

            // Burada da mesaj geldiğinde yapacağımız eventi belirtiyoruz.
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                Thread.Sleep(1500);
                Console.WriteLine("Gelen Mesaj: " + message);

                // Mesajı işledik diye rabbitmq'ye bilgi verdik, ilgili mesajın tag'ini gönderdik. İkinci parametreyi false yaparak daha önceden işlediğim ve rabbitmq'ye haber vermediğim şeyleri de göndereyim mi diyor, ona false dedik, sadece bunu göndersin.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

        void handleTopicExchange()
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);

            // Random bir queue name oluşturduk, route olarak da başı ve sonu önemli değil sadece ortasında Error olan route'ı olan mesajları al diyoruz.
            var queueName = channel.QueueDeclare().QueueName;
            //var routeKey = "*.Error.*";

            // Eğer diyez koyarsak başı hiç önemli değil, sonu warning olsun diyoruz. Topicleri nokta ile ayırıyoruz. 
            // * bir topic'i temsil eder, # ise sol taraftaki tüm topicleri istersek 10 tane olsun farketmez.
            var routeKey = "#.Warning";

            // Burada da bir queue bind ettik, consumer kapandığında queue da silinecek. ilgili route'u ve excahnge ismini verdik.
            channel.QueueBind(queueName, "logs-topic", routeKey, null);

            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Loglar dinleniyor...");

            // Burada da mesaj geldiğinde yapacağımız eventi belirtiyoruz.
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                Thread.Sleep(1500);
                Console.WriteLine("Gelen Mesaj: " + message);

                // Mesajı işledik diye rabbitmq'ye bilgi verdik, ilgili mesajın tag'ini gönderdik. İkinci parametreyi false yaparak daha önceden işlediğim ve rabbitmq'ye haber vermediğim şeyleri de göndereyim mi diyor, ona false dedik, sadece bunu göndersin.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }
    }
}
