using EarthaquakeApplication.Entities;
using EarthaquakeInfrastructure.Data;
using EarthaquakeApplication.Entities;
using EarthaquakeInfrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EarthquakeApi.Controllers
{
    [Authorize]
    public class EarthquakeListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EarthquakeListController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(double? minMagnitude, string? location, string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;

            ViewBag.MagnitudeSort = sortOrder == "magnitude_asc" ? "magnitude_desc" : "magnitude_asc";
            ViewBag.DepthSort = sortOrder == "depth_asc" ? "depth_desc" : "depth_asc";
            ViewBag.DateSort = sortOrder == "date_asc" ? "date_desc" : "date_asc";

            IQueryable<EarthquakeModel> earthquakesQuery = _context.Earthquakes;

            if (!string.IsNullOrEmpty(location))
            {
                earthquakesQuery = earthquakesQuery.Where(e =>
                    e.Location.ToLower().Contains(location.ToLower()) ||
                    (e.District != null && e.District.ToLower().Contains(location.ToLower()))
                );
            }

            switch (sortOrder)
            {
                case "magnitude_asc":
                    earthquakesQuery = earthquakesQuery.OrderBy(e => Convert.ToDouble(e.Magnitude));
                    break;
                case "magnitude_desc":
                    earthquakesQuery = earthquakesQuery.OrderByDescending(e => Convert.ToDouble(e.Magnitude));
                    break;
                case "depth_asc":
                    earthquakesQuery = earthquakesQuery.OrderBy(e => Convert.ToDouble(e.Depth));
                    break;
                case "depth_desc":
                    earthquakesQuery = earthquakesQuery.OrderByDescending(e => Convert.ToDouble(e.Depth));
                    break;
                case "date_asc":
                    earthquakesQuery = earthquakesQuery.OrderBy(e => e.Date);
                    break;
                case "date_desc":
                    earthquakesQuery = earthquakesQuery.OrderByDescending(e => e.Date);
                    break;
                default:
                    earthquakesQuery = earthquakesQuery.OrderByDescending(e => e.Date);
                    break;
            }

            var allEarthquakes = await earthquakesQuery.ToListAsync();

            if (minMagnitude.HasValue)
            {
                allEarthquakes = allEarthquakes.Where(e =>
                {
                    float magnitudeFloat;
                    return float.TryParse(e.Magnitude, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out magnitudeFloat)
                           && magnitudeFloat >= minMagnitude.Value;
                }).ToList();
            }

            ViewBag.MinMagnitude = minMagnitude;
            ViewBag.Location = location;

            return View(allEarthquakes);
        }
    }
}