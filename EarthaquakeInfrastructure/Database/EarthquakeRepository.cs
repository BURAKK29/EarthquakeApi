using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;


namespace EarthaquakeInfrastructure.Database
{
    public class EarthquakeRepository : IEarthquakeRepository
    //veri tabanına yazma işini tam olarak bu yapmaz
    {//burada daha çok kontrol yapılır sonra herşey okeyse datalar tutulur.Yazma işini EartquakeService yapar.
        private readonly IDbConnection _connection;

        public EarthquakeRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task SaveManyAsync(List<EarthquakeModel> earthquakes)
        {

            foreach (var earthquake in earthquakes)
            {
                var existing = await _connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM Earthquakes WHERE EventId=@EventID",
                    new { earthquake.EventID });

                if (existing > 0)
                {
                    continue;
                }
                await _connection.ExecuteAsync(@"
                    INSERT INTO Earthquakes(EventId, Location, Latitude, Longitude, Depth,
                    Type, Magnitude, Country, Province, District,
                    Neighborhood, Date, IsEventUpdate)
                    VALUES(@EventID, @Location, @Latitude, @Longitude, @Depth,
                    @Type, @Magnitude, @Country, @Province, @District,
                    @Neighborhood, @Date, @IsEventUpdate)", earthquakes);
            }
        }

        public async Task<List<EarthquakeModel>> GetAllAsync()
        {
            var result = await _connection.QueryAsync<EarthquakeModel>(
            "SELECT * FROM Earthquakes ORDER BY Date DESC");
            //QueryAsync, veri okur nesne olarak döndürür. Bu nesneler bir koleksiyonda döner = IEnumerable<Earthquake>
            return result.ToList();
        }
        public async Task<bool> CheckIfExistAsync(string eventId)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {                                                                  //QueryFirstOrDefaultAsync:
                    var count = await _connection.QueryFirstOrDefaultAsync<int>(  //Gelen ilk sonucu alır.
                        "SELECT COUNT(*) FROM Earthquakes WHERE EventId=@EventId",
                        new { EventId = eventId });
                    return count > 0;
                }
                catch (SqlException)
                {
                    await Task.Delay(2000);
                }
            }
            throw new Exception("SQL server bağlantısı kurulamadı. ");
        }


    }
}
