using ConsoleTables;
using exporter.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

string? connectionString = configuration.GetConnectionString("MyDatabase");

var options = new DbContextOptionsBuilder<MyDBContext>()
    .UseSqlServer(connectionString)
    .Options;

using (var dbContext = new MyDBContext(options))
{
    var sensorsWithMeasurementCounts = dbContext.Sensor
        .Select(sensor => new
        {
            sensor.DomainId,
            sensor.Type.ValueType,
            MeasurementsCount = sensor.Measurements.Count()
        })
        .ToList();

    var table = new ConsoleTable("DomainId", "ValueType", "MeasurementsCount");

    foreach (var s in sensorsWithMeasurementCounts)
    {
        table.AddRow(s.DomainId, s.ValueType, s.MeasurementsCount);
    }

    table.Write();
}
