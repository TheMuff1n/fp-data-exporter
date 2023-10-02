using exporter.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exporter.Data
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options) { }

        public DbSet<Station> Stations { get; set; }

        public DbSet<Sensor> Sensors { get; set; }

        public DbSet<SensorType> SensorTypes { get; set; }

        public DbSet<Measurement> Measurements { get; set; }
    }
}
