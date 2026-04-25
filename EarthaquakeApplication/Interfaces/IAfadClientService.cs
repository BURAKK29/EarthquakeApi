using EarthaquakeApplication.Entities;


namespace EarthaquakeApplication.Interfaces
{
    public interface IAfadClientService
    {
        Task<List<EarthquakeModel>> GetEarthquakesAsync();

    }
}
