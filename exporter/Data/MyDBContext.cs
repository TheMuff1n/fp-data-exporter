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

        public DbSet<Station> Station { get; set; }

        public DbSet<Sensor> Sensor { get; set; }

        public DbSet<SensorType> SensorType { get; set; }

        public DbSet<Measurement> Measurement { get; set; }
    }
}
