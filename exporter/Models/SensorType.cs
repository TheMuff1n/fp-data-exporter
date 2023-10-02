using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exporter.Models
{
    public class SensorType
    {
        [Key]
        public Guid Id { get; set; }

        public string DomainId { get; set; }

        public string Unit {  get; set; }

        public string ValueType { get; set; }

        public int PredictionHorizonInHours { get; set; }
    }

    public class SensorTypeIds
    {
        public const int LEVEL = 0010;

        public const int LEVEL_PREDICTION_2H = 10001;
        public const int LEVEL_PREDICTION_3H = 10002;
        public const int LEVEL_PREDICTION_4H = 10003;

        public const int RAINFALL = 0101;

        public const int AIR_TEMPERATURE = 0130;

        public const int SNOW_HEIGHT = 6030;

        public const int HUMIDITY = 0300;

        public const int FLOW = 0200;

        public const int V_FLOW = 3500;
    }
}
