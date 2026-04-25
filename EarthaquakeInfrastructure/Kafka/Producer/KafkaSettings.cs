using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarthaquakeInfrastructure.Kafka.Producer
{
    public class KafkaSettings
    {
        public string VoostrapServers { get; set; } = "";
        public string Topic { get; set; } = "";
    }
}
