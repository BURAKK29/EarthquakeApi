using EarthaquakeApplication.Entities;

namespace EarthaquakeApplication.Interfaces
{
    public interface IEarthquakeService
    {
        Task SyncEarthquakesAsync();
        Task SaveFromKafkaAsync(EarthquakeModel earthquake);
    }
}
