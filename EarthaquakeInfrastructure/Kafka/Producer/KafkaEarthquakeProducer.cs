using EarthaquakeInfrastructure.Kafka.Message;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.Extensions.Options;


namespace EarthaquakeInfrastructure.Kafka.Producer
{
    public class KafkaEarthquakeProducer : IKafkaEarthquakeProducer
    {
        private readonly IMessageProducer _producer;//kafkaya mesaj gönderen servis
        private readonly string _topic;
        public KafkaEarthquakeProducer(IProducerAccessor accessor, IOptions<KafkaSettings> options)//kafkanın erişim sağlama servisi
        {
            _producer = accessor.GetProducer("earthquake-producer");
            _topic = options.Value.Topic;
        }

        public async Task ProduceAsync(EarthquakeMessage message)
        {
            Console.WriteLine($"[Producer] Sending message: {message.EventID}");
            await _producer.ProduceAsync(
                topic: _topic,
                messageKey: message.EventID,
                message);
            Console.WriteLine($"[Producer] Sent to topic earthquake-topic");
        }
    }
}
