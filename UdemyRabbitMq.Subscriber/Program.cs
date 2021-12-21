using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace UdemyRabbitMq.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(channel);

            var queueName = channel.QueueDeclare().QueueName;

            // Consumer tarafında da header oluşturduk ve header'ında istediğimiz bilgiler olan mesajı alacağız.
            Dictionary<string, object> headers = new Dictionary<string, object>();

            // Header'ında şunlar olsun dedik, pdf vs.
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            // En son da x-match key'i ile bir header belirtiyoruz. Eğer değerini all dersek yukarda belirttiğimiz tüm verileri karşılayan mesajı arar ve alır. Eğer any dersek verdiğimiz değerlerden herhangi uyuşan birini arar ve bulur.
            headers.Add("x-match", "all");
            //  headers.Add("x-match", "any");

            // Bir queue bind ettik, exchange'e bağlandık, ve headers'ıan bizim headers'ı ekledik.
            channel.QueueBind(queueName, "header-exchange", string.Empty, headers);

            channel.BasicConsume(queueName, false, consumer);

            Console.WriteLine("Mesaj dinleniyor...");

            // Burada da mesaj geldiğinde yapacağımız eventi belirtiyoruz.
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray());

                // Consumer tarafında da gelen jsonString'i bu şekilde Deserialize ederek product nesnesine dönüştürdük.
                var product = JsonSerializer.Deserialize<Product>(message);

                Thread.Sleep(1500);
                Console.WriteLine($"Gelen Product: {product.Id} - {product.Name} - {product.Price} - {product.Stock}");

                // Mesajı işledik diye rabbitmq'ye bilgi verdik, ilgili mesajın tag'ini gönderdik. İkinci parametreyi false yaparak daha önceden işlediğim ve rabbitmq'ye haber vermediğim şeyleri de göndereyim mi diyor, ona false dedik, sadece bunu göndersin.
                channel.BasicAck(e.DeliveryTag, false);
            };

            Console.ReadLine();
        }

    }
}






//var channel = connection.CreateModel();

//// Bu şekilde rabbitmq içinden random bir queue name oluşturuyoruz ki her consumer kendi benzersiz queue'unu dinlesin.
//var randomQueueName = channel.QueueDeclare().QueueName;

//// Burada da bir kuyruğu logs-fanout exchange'imize bind ettik. Bunun olayı şu: Direk QueueDeclare deseydik bir kuyruk oluşturacaktık ve exchange'i silsek dahi queue orada kalacaktı, bu senaryoda bu mantıksız olur çünkü bu queue'nin sadece bu exchange ile ilişkili olmasını istiyorum. Bu yüzden declare değil de direk QueueBind diyorum ve exchange'i veriyorum. Bu şekilde yapınca eğer exchange silinirse queue de silinir ve boşa yer kaplamamış olur.
//channel.QueueBind(randomQueueName, "logs-fanout", "", null);
//            channel.BasicQos(0, 1, false);

//            // Bir consumer yani alıcı oluşturduk.
//            var consumer = new EventingBasicConsumer(channel);

//// Burada yukarıda bind ettiğimiz queue'ye consume olduk ve kuyruğu dinliyicez. İkinci parametreyi false yaparak direk mesajı alır almaz okudum, işledim deyip kuyruktan silmesin diye böyle yaptık. Biz mesajı doğru işleyince rabbitmq'ye haber vereceğiz.
//channel.BasicConsume(randomQueueName, false, consumer);

//            Console.WriteLine("Loglar dinleniyor...");

//            // Burada da mesaj geldiğinde yapacağımız eventi belirtiyoruz.
//            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
//            {
//                var message = Encoding.UTF8.GetString(e.Body.ToArray());

//Thread.Sleep(1500);
//                Console.WriteLine("Gelen Mesaj: " + message);

//                // Mesajı işledik diye rabbitmq'ye bilgi verdik, ilgili mesajın tag'ini gönderdik. İkinci parametreyi false yaparak daha önceden işlediğim ve rabbitmq'ye haber vermediğim şeyleri de göndereyim mi diyor, ona false dedik, sadece bunu göndersin.
//                channel.BasicAck(e.DeliveryTag, false);
//            };














// Burada istediğimiz kadar mesaj alabiliriz. Bir consumer'ın ne kadar mesaj alacağını belirtiyoruz. İlk baştaki 0 bir boyut kısıtlamanız var mı diye soruluyor, 0 diyerek her boyuttakini alabiliriz diyoruz. 
// İkinci parametrede de tek seferde kaç mesaj alacağımızı belirtiyoruz. 
// Üçüncü parametrede ise bu sayıyı global olarak mı dağıtacağını yoksa her bir consumer'a belirtilen sayı kadar mı dağıtacağını gösterir. Örneğin 10 mesaj dağıtsın deyip global kısmını false yaparsak her bir consumer'a 10 adet mesaj verir. Eğer global'i true yaparsak 2 consumer varsa her bir consumer'a 5 tane mesaj verir.
//channel.BasicQos(0, 1, false);

//// Bir consumer yani alıcı oluşturduk.
//var consumer = new EventingBasicConsumer(channel);

//// Burada queue'ye consume oluyoruz ve mesajları almak istiyoruz. Burada ikinci parametreyi false yaparak direk mesajı alır almaz okudum, işledim deyip kuyruktan silmesin diye böyle yaptık. Biz mesajı doğru işleyince rabbitmq'ye haber vereceğiz.
//channel.BasicConsume("hello-queue", false, consumer);

//// Burada da mesaj geldiğinde yapacağımız eventi belirtiyoruz.
//consumer.Received += (object sender, BasicDeliverEventArgs e) =>
//{
//var message = Encoding.UTF8.GetString(e.Body.ToArray());

//Thread.Sleep(1500);
//Console.WriteLine("Gelen Mesaj: " + message);

//// Mesajı işledik diye rabbitmq'ye bilgi verdik, ilgili mesajın tag'ini gönderdik. İkinci parametreyi false yaparak daha önceden işlediğim ve rabbitmq'ye haber vermediğim şeyleri de göndereyim mi diyor, ona false dedik, sadece bunu göndersin.
//channel.BasicAck(e.DeliveryTag, false);
//};
