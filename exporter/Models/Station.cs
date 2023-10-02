using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace exporter.Models
{
    public class Station
    {
        [Key]
        [JsonIgnore]
        public Guid Id { get; set; }

        public int DomainId { get; set; }

        public string Name { get; set; }

        public float Longitude { get; set; }

        public float Latitude { get; set; }

        public ICollection<Sensor> Sensors { get; set; }
    }

    public class StationIds
    {
        public const int WS_GRANETALSPERRE = 5000;

        public const int WS_GOSETAL = 150424;

        public const int WS_WINTERTAL = 150423;

        public const int GOSETAL_UW = 48211160;

        public const int WINTERTAL = 48211250;

        public const int RAMMELSBERGHAUS = 48211350;

        public const int MARGARETHENKLIPPE = 48861030;

        public const int WS_HAHNENKLEE = 150422;
    }
}
