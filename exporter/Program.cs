using CommandLine;
using ConsoleTables;
using exporter.Data;
using Microsoft.EntityFrameworkCore;

class Program
{
    class Options
    {
        [Value(0, MetaName = "connectionString", HelpText = "Connection string for the database connection.", Required = true)]
        public string ConnectionString { get; set; }

        [Option('f', "format", Default = OutputFormat.Preview, HelpText = "Output format (csv or human-readable preview).")]
        public OutputFormat Format { get; set; }

        [Option("from", HelpText = "Starting date to fetch data from.")]
        public DateTime FromDate { get; set; } = DateTime.MinValue;

        [Option("to", HelpText = "Ending date to fetch data from.")]
        public DateTime ToDate { get; set; } = DateTime.MaxValue;
    }

    enum OutputFormat
    {
        Preview,
        Csv
    }

    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(Run)
            .WithNotParsed(HandleParseError);
    }

    static void Run(Options options)
    {
        Console.WriteLine($"Loading data from source '{options.ConnectionString}'.");
        Console.WriteLine($"Displaying data from {options.FromDate} to {options.ToDate} formatted as {options.Format}.");

        var dbOptions = new DbContextOptionsBuilder<MyDBContext>()
            .UseSqlServer(options.ConnectionString)
            .Options;

        using (var dbContext = new MyDBContext(dbOptions))
        {
            var sensorsWithMeasurementCounts = dbContext.Sensor
                .Select(sensor => new
                {
                    sensor.DomainId,
                    sensor.Type.ValueType,
                    MeasurementsCount = sensor.Measurements
                        .Where(m => options.FromDate <= m.MeasuredAt && m.MeasuredAt <= options.ToDate)
                        .Count()
                })
                .ToList();

            var table = new ConsoleTable("DomainId", "ValueType", "MeasurementsCount");

            foreach (var s in sensorsWithMeasurementCounts)
            {
                table.AddRow(s.DomainId, s.ValueType, s.MeasurementsCount);
            }

            table.Write();
        }
    }

    static void HandleParseError(IEnumerable<Error> errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error}");
        }
    }
}
