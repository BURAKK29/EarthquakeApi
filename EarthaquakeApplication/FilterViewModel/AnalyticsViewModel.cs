using EarthaquakeApplication.Entities;
using System.Collections.Generic;

namespace EarthaquakeApplication.FilterViewModel
{
    public class AnalyticsViewModel
    {
        public int TotalEarthquakes { get; set; }
        public double MaxMagnitude { get; set; }
        public double AverageMagnitude { get; set; }
        
        // Şehirlere Göre Deprem Sayısı (Pasta Grafik için)
        public Dictionary<string, int> TopCities { get; set; } = new Dictionary<string, int>();

        // Son 7 Günün Deprem Sayısı Trendi (Çizgi Grafik için)
        public Dictionary<string, int> DailyTrends { get; set; } = new Dictionary<string, int>();

        // Son 10 Deprem Listesi
        public List<EarthquakeModel> RecentEarthquakes { get; set; } = new List<EarthquakeModel>();
    }
}
