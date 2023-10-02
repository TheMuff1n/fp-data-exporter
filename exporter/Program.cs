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

        [Option("sensors", HelpText = "Sensors to fetch data from, seperated by commas (e.g. 5000:10,5000:130).")]
        public IEnumerable<string> SelectedSensors { get; set; }

        [Option('f', "format", Default = OutputFormat.Preview, HelpText = "Output format (Summary, Preview or Csv).")]
        public OutputFormat Format { get; set; }

        [Option("from", HelpText = "Starting date to fetch data from.")]
        public DateTime FromDate { get; set; } = DateTime.MinValue;

        [Option("to", HelpText = "Ending date to fetch data from.")]
        public DateTime ToDate { get; set; } = DateTime.MaxValue;
    }

    enum OutputFormat
    {
        Summary,
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
        Console.WriteLine($"Selected sensors: {(options.SelectedSensors.Count() == 0 ? "All" : string.Join(", ", options.SelectedSensors))}.");

        var dbOptions = new DbContextOptionsBuilder<MyDBContext>()
            .UseSqlServer(options.ConnectionString)
            .Options;

        using (var dbContext = new MyDBContext(dbOptions))
        {
            switch (options.Format)
            {
                case OutputFormat.Summary:
                    ShowSummary(dbContext, options.FromDate, options.ToDate); break;
                case OutputFormat.Preview:
                    ShowPreview(dbContext, options.FromDate, options.ToDate, options.SelectedSensors); break;
                case OutputFormat.Csv:
                    SaveCsv(dbContext, options.FromDate, options.ToDate);  break;
            }
        }
    }

    static void HandleParseError(IEnumerable<Error> errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error}");
        }
    }

    static void ShowSummary(MyDBContext dbContext, DateTime fromDate, DateTime toDate)
    {
        var sensorsWithMeasurementCounts = dbContext.Sensor
            .Select(sensor => new
            {
                sensor.DomainId,
                sensor.Type.ValueType,
                MeasurementsCount = sensor.Measurements
                .Where(m => fromDate <= m.MeasuredAt && m.MeasuredAt <= toDate)
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

    static void ShowPreview(MyDBContext dBContext, DateTime fromDate, DateTime toDate, IEnumerable<string> selectedSensors)
    {
        var sensorsWithMeasurements = dBContext.Sensor
            .Where(s => selectedSensors.Count() == 0 || selectedSensors.Contains(s.DomainId))
            .Select(s => new
            {
                s.DomainId,
                Measurements = s.Measurements
                    .Where(m => !m.IsObsolete && fromDate <= m.MeasuredAt && m.MeasuredAt < toDate)
                    .Select(m => new
                    {
                        m.MeasuredAt,
                        m.MeasuredValue
                    })
            })
            .ToList();

        //TODO:set from and to to 15 minute fixed
        var timestamps = new List<DateTime>();
        var currentTimestamp = fromDate;
        while (currentTimestamp < toDate)
        {
            timestamps.Add(currentTimestamp);
            currentTimestamp = currentTimestamp.AddMinutes(15);
        }

        var tableHead = sensorsWithMeasurements
            .Select(s => s.DomainId)
            .Prepend("MeasuredAt")
            .ToArray();

        var tableBody = timestamps.Select(t => new
        {
            MeasuredAt = t,
            Measurements = sensorsWithMeasurements
                .Select(s => s.Measurements.FirstOrDefault(m => m.MeasuredAt == t)?.MeasuredValue)
                .ToList()
        })
            .ToList();

        var table = new ConsoleTable(tableHead);

        foreach (var r in tableBody)
        {
            table.AddRow(r.Measurements.Select(m => string.Format("{0:F2}", m)).Prepend(r.MeasuredAt.ToString()).ToArray());
        }

        table.Write();
    }

    static void SaveCsv(MyDBContext dBContext, DateTime fromDate, DateTime toDate)
    {
        //TODO
    }
}
