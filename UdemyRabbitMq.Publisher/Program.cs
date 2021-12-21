using RabbitMQ.Client;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace UdemyRabbitMq.Publisher
{
    class Program
    {
        static void Main(string[] args)
        {
            // Önce bir factory açtık ve sunucudaki rabbitmq'ye bağlandık.
            var factory = new ConnectionFactory();
            factory.Uri = new Uri("");
            using var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            // Burada bir exchange oluşturduk ve tipi headers tipi. Bu tipte header'da bilgiler gönderiyoruz ve consumer da almak istediği mesajın header'ındakileri gönderiyor, headerlar eşleşiyorsa ilgili mesajı alıyor.
            channel.ExchangeDeclare("header-exchange", durable: true, type: ExchangeType.Headers);

            // Bir dictionary oluşturduk ve header'a koyacaklarımızı belirttik.
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            // RabbitMq için properties oluşturduk ve header'ına istediğimiz header'ı verdik.
            var properties = channel.CreateBasicProperties();
            properties.Headers = headers;

            // Properties'lere persistant true'ya set edersek kuyruk ve exchange haricinde mesajlar da kalıcı hale gelir.
            properties.Persistent = true;

            // Burada bir product nesnesi oluşturduk ve onu jsonString'e çevirdik.
            var product = new Product { Id = 1, Name = "Ahmet Abi", Price = 100.0, Stock = 199 };
            var productJsonString = JsonSerializer.Serialize(product);

            // Daha sonra burada jsonstring'in byte'larını alıp mesaj değişkenine atadık.
            var message = Encoding.UTF8.GetBytes(productJsonString);

            // Burada da mesajı ilettik.
            channel.BasicPublish("header-exchange", string.Empty, properties, message);

            Console.WriteLine($"Mesaj gönderilmiştir.");

            Console.ReadLine();

        }
    }
}


//// Önce bir factory açtık ve sunucudaki rabbitmq'ye bağlandık.
//var factory = new ConnectionFactory();
//factory.Uri = new Uri("amqps://auquzjtr:Evb7eHXP46nBaFkehBraF8Xw_tRjOGwZ@fox.rmq.cloudamqp.com/auquzjtr");
//            using var connection = factory.CreateConnection();

//            // Daha sonra bağlantımızdan bir channal açtık. Bir bağlantı kurmak maliyetlidir, sunucu vs gerektirir, fakat bağlı bağlantıdan birsürü channel oluşturup onun üzerinden işlem yapılabilir.
//            var channel = connection.CreateModel();

//// Burada bir kuyruk oluşturuyoruz. İlk parametres olan true, kuyruk memory'de mi tutulsun yoksa biryere kaydedilip ordan mı okunsun diye soruyor. Memory'de tutulursa uygulama kapandığında kuyruktaki bilgiler gider. Bu yüzden true diyoruz ki memory'de tutulmasın db'de tutulsun diyoruz.
//// İkinci parametre olan false'ta da şunu belirtiyoruz. Bu kuyruğa sadece şuan oluşturduğum channel üzerinden mi bağlanılsın diye soruyor. Ama benim amacım bunu başka bir projede başka bir channel'dan da kullanmak. Bu yüzden false diyoruz.
//// Üçüncü false'ta da kuyruklara subscribe olan son subscriber da bağlantısı koparırsa kuyruğu siler. Bu da çok mantıklı değil, bu yüzden false diyoruz
//channel.QueueDeclare("hello-queue", true, false, false);

//Enumerable.Range(1, 50).ToList().ForEach(x =>
//{
//string message = $"Mesaj: {x}";
//var messageBody = Encoding.UTF8.GetBytes(message);

//channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);

//Console.WriteLine($"Mesaj gönderilmiştir: {message}");
//});

//Console.ReadLine();


//string message = "hello Ahmet abi555";
//var messageBody = Encoding.UTF8.GetBytes(message);

//// Biz eğer exchange kullanmazsak burada empty olarak belirttik fakat rabbit mq tarafında default exchange kullanıyor gibi görünürüz. Bu yüzden ikinci parametre olan routingKey'e kuyruğun ismini vermemiz gerekiyor. Bu sayede routeMap'e göre kuyruğa ilgili mesajı iletir.
//// Ayarları null yaptık, mesaj olarak da messageBody'i verdik.

//// RabbitMq ile sadece byte dizisi transfer yapabiliriz. Bu sayede text, image, pdf vb. tüm türleri byte dizisine çevirip gönderebiliriz.
//channel.BasicPublish(string.Empty, "hello-queue", null, messageBody);

//Console.WriteLine("Mesaj gönderilmiştir.");
