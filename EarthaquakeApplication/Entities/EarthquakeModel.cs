using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthaquakeApplication.Entities
{
    public class EarthquakeModel
    {
        public int Id { get; set; }
        public string EventID { get; set; }
        public string? Location { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Depth { get; set; }
        public string? Magnitude { get; set; }
        public string? Type { get; set; }
        public string? Country { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public string? Neighborhood { get; set; }
        public DateTime Date { get; set; }
        public bool? IsEventUpdate { get; set; }
    }
}
