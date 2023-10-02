using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exporter.Models
{
    public class Sensor
    {
        [Key]
        public Guid Id { get; set; }

        public string DomainId { get; set; }

        public SensorType Type { get; set; }

        public IEnumerable<Measurement> Measurements { get; set; }
    }
}
