using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthaquakeApplication.Queries
{
    public class GetAllEarthquakesResponse
    {
        public string EventID { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Depth { get; set; }
        public string Type { get; set; }
        public double Magnitude { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Neighborhood { get; set; }
        public DateTime Date { get; set; }
        public bool IsEventUpdate { get; set; }
        public DateTime? LastUpdateDate { get; set; }
    }
}
