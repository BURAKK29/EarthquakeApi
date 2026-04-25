using EarthaquakeApplication.Entities;
using EarthaquakeApplication.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;

namespace EarthaquakeApplication.Queries
{
    public class GetAllEarthquakesQueryHandler : IRequestHandler<GetAllEarthquakesQuery, List<GetAllEarthquakesResponse>>
    {
        private readonly IEarthquakeRepository repository;
        public GetAllEarthquakesQueryHandler(IEarthquakeRepository repository)
        {
            this.repository = repository;
        }

        public async Task<List<GetAllEarthquakesResponse>> Handle(GetAllEarthquakesQuery request, CancellationToken cancellationToken)
        {
            var entities = await repository.GetAllAsync();

            return entities.Select(e => new GetAllEarthquakesResponse
            {
                Location = e.Location,
                Magnitude = double.Parse(e.Magnitude.ToString(), CultureInfo.InvariantCulture),
                Date = e.Date
            }).ToList();
        }
    }
}
