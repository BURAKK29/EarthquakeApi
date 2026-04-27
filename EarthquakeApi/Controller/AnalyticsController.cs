using EarthaquakeApplication.Entities;
using EarthaquakeInfrastructure.Data;
using EarthaquakeApplication.FilterViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace EarthquakeApi.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allEarthquakes = await _context.Earthquakes.ToListAsync();

            var viewModel = new AnalyticsViewModel();

            if (allEarthquakes.Any())
            {
                viewModel.TotalEarthquakes = allEarthquakes.Count;

                var parsedMagnitudes = allEarthquakes
                    .Select(e =>
                    {
                        double.TryParse(e.Magnitude, NumberStyles.Any, CultureInfo.InvariantCulture, out double m);
                        return m;
                    })
                    .ToList();

                viewModel.MaxMagnitude = parsedMagnitudes.Max();
                viewModel.AverageMagnitude = Math.Round(parsedMagnitudes.Average(), 2);

                // En çok deprem olan 10 şehir (Pie Chart)
                viewModel.TopCities = allEarthquakes
                    .Where(e => !string.IsNullOrEmpty(e.Location))
                    .GroupBy(e => 
                    {
                        var parts = e.Location.Split('(');
                        return parts.Length > 1 ? parts[1].Replace(")", "").Trim() : "Bilinmiyor";
                    })
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Son 7 gün trendi (Line Chart)
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                var recentEarthquakes = allEarthquakes.Where(e => e.Date >= sevenDaysAgo).ToList();

                var groupedDates = recentEarthquakes
                    .GroupBy(e => e.Date.Date)
                    .OrderBy(g => g.Key)
                    .ToDictionary(g => g.Key.ToString("dd MMM"), g => g.Count());

                // Tüm haftayı doldur (Boş günleri 0 yap)
                for (int i = 6; i >= 0; i--)
                {
                    var dateStr = DateTime.UtcNow.AddDays(-i).Date.ToString("dd MMM");
                    if (!groupedDates.ContainsKey(dateStr))
                    {
                        groupedDates[dateStr] = 0;
                    }
                }
                
                // Sıralamayı garantiye al
                viewModel.DailyTrends = groupedDates.OrderBy(kv => DateTime.ParseExact(kv.Key, "dd MMM", CultureInfo.InvariantCulture)).ToDictionary(kv => kv.Key, kv => kv.Value);

                // Son 10 Deprem
                viewModel.RecentEarthquakes = allEarthquakes
                    .OrderByDescending(e => e.Date)
                    .Take(10)
                    .ToList();
            }

            return View(viewModel);
        }

        public IActionResult Heatmap()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetHeatmapData()
        {
            // Sadece konum ve büyüklük bilgilerini JSON dönüyoruz
            var data = await _context.Earthquakes
                .Where(e => !string.IsNullOrEmpty(e.Latitude) && !string.IsNullOrEmpty(e.Longitude))
                .Select(e => new
                {
                    lat = e.Latitude,
                    lng = e.Longitude,
                    mag = e.Magnitude
                })
                .ToListAsync();

            return Json(data);
        }
    }
}
