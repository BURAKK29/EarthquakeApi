using EarthaquakeApplication.Entities;

namespace EarthaquakeApplication.Interfaces
{
    public interface IEarthquakeRepository
    {
        Task<List<EarthquakeModel>> GetAllAsync();
        Task SaveManyAsync(List<EarthquakeModel> earthquakes);
        Task<bool> CheckIfExistAsync(string eventId);
    }
}
