using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthaquakeInfrastructure.Kafka.Message
{
    public class EarthquakeMessage
    {
        public string EventID { get; set; }
        public string Location { get; set; }
        public string Magnitude { get; set; }
        public DateTime Date { get; set; }

    }
}
