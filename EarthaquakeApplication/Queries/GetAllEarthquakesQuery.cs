using MediatR;

namespace EarthaquakeApplication.Queries
{
    public class GetAllEarthquakesQuery : IRequest<List<GetAllEarthquakesResponse>>
    {

    }
}
