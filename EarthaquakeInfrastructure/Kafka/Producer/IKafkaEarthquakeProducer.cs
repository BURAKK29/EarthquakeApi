using EarthaquakeInfrastructure.Kafka.Message;

namespace EarthaquakeInfrastructure.Kafka.Producer
{
    public interface IKafkaEarthquakeProducer//mesaj gönderen servis
    {
        Task ProduceAsync(EarthquakeMessage message);
    }
}
