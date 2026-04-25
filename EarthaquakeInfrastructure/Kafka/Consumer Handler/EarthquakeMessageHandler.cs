using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using EarthaquakeInfrastructure.Kafka.Message;
using KafkaFlow;
using Microsoft.Extensions.DependencyInjection;


namespace EarthaquakeInfrastructure.Kafka.Consumer_Handler
{
    public class EarthquakeMessageHandler : IMessageHandler<EarthquakeMessage>
    {

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EarthquakeMessageHandler(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }


        public async Task Handle(IMessageContext context, EarthquakeMessage message)
        {
            Console.WriteLine($"[KAFKA - CONSUMER] Gelen mesaj: {message.EventID} - {message.Location} - {message.Date}");

            using var scope = _serviceScopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IEarthquakeRepository>();

            var existing = await repository.CheckIfExistAsync(message.EventID);
            if (!existing)
            {
                var earthquake = new EarthquakeModel
                {
                    EventID = message.EventID,
                    Location = message.Location,
                    Magnitude = message.Magnitude.ToString(),
                    Date = message.Date
                };

                await repository.SaveManyAsync(new List<EarthquakeModel> { earthquake });
                Console.WriteLine($"[KAFKA- DB]Yeni deprem kaydı veritabanına eklendi: {earthquake.EventID}");
            }
            else
            {
                Console.WriteLine($"[KAFKA - DB] Bu deprem zaten kayıtlı: {message.EventID}");
            }
        }

    }
}
