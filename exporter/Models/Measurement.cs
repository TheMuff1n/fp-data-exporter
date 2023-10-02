using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace exporter.Models
{
    public class Measurement
    {
        [Key]
        [JsonIgnore]
        public Guid Id { get; set; }

        public double MeasuredValue { get; set; }

        public DateTime MeasuredAt { get; set; }

        public DateTime ReceivedAt { get; set; }

        public bool IsObsolete { get; set; }

        [ForeignKey(nameof(Sensor))]
        [JsonIgnore]
        public Guid SensorId { get; set; }

        [JsonIgnore]
        public Sensor Sensor { get; set; }
    }
}
