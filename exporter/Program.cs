using CommandLine;
using ConsoleTables;
using CsvHelper;
using CsvHelper.Configuration;
using exporter.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        [Option('o', "output", HelpText = "The output filepath to write to. If not specified, the output is stdout.")]
        public string OutputPath { get; set; }
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
                    SaveCsv(dbContext, options.FromDate, options.ToDate, options.SelectedSensors, options.OutputPath);  break;
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
        var currentTimestamp = sensorsWithMeasurements
            .Select(s => s.Measurements.MinBy(m => m.MeasuredAt))
            .MinBy(s => s.MeasuredAt)?.MeasuredAt ?? fromDate;
        var lastTimestamp = sensorsWithMeasurements
            .Select(s => s.Measurements.MaxBy(m => m.MeasuredAt))
            .MaxBy(s => s.MeasuredAt)?.MeasuredAt ?? toDate;
        while (currentTimestamp <= lastTimestamp)
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

    static void SaveCsv(MyDBContext dBContext, DateTime fromDate, DateTime toDate, IEnumerable<string> selectedSensors, string outputPath)
    {
        var measurementsGrouped = dBContext.Sensor
            .Where(s => selectedSensors.Count() == 0 || selectedSensors.Contains(s.DomainId))
            .Select(s => new
            {
                s.DomainId,
                Measurements = s.Measurements
                    .Where(m => !m.IsObsolete && fromDate <= m.MeasuredAt && m.MeasuredAt < toDate)
                    .Select(m => new
                    {
                        s.DomainId,
                        m.MeasuredAt,
                        m.MeasuredValue,
                        m.ReceivedAt
                    })
                    .GroupBy(m => m.ReceivedAt)
                    .Select(g => g.OrderByDescending(m => m.ReceivedAt).First())
            })
            .ToList();

        var headers = new List<string> { "MeasuredAt" };
        var uniqueDomainIds = measurementsGrouped.Select(s => s.DomainId).Distinct();
        headers.AddRange(uniqueDomainIds);
        var domainIdToIndex = headers
            .Select((header, index) => new { Header = header, Index = index })
            .ToDictionary(item => item.Header, item => item.Index);

        var rows = measurementsGrouped
            .SelectMany(s => s.Measurements)
            .GroupBy(m => m.MeasuredAt)
            .OrderBy(group => group.Key)
            .Select(group =>
            {
                var row = Enumerable.Repeat<object>("", 33).ToArray();
                row[0] = group.Key;
                foreach (var m in group)
                {
                    if (domainIdToIndex.TryGetValue(m.DomainId, out var index))
                    {
                        row[index] = m.MeasuredValue;
                    }
                }
                return row;
            })
            .ToList();

        //TODO:set from and to to 15 minute fixed

        var minMeasuredAt = measurementsGrouped
            .SelectMany(s => s.Measurements)
            .Select(m => m.MeasuredAt)
            .DefaultIfEmpty(DateTime.MaxValue)
            .Min();

        var maxMeasuredAt = measurementsGrouped
            .SelectMany(s => s.Measurements)
            .Select(m => m.MeasuredAt)
            .DefaultIfEmpty(DateTime.MinValue)
            .Max();

        var timestamps = new List<DateTime>();
        var currentTimestamp = minMeasuredAt;
        while (currentTimestamp <= maxMeasuredAt)
        {
            timestamps.Add(currentTimestamp);
            currentTimestamp = currentTimestamp.AddMinutes(15);
        }

        using (var writer = new StreamWriter(outputPath))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
        {
            foreach (var columnHeader in headers)
            {
                csv.WriteField(columnHeader);
            }
            csv.NextRecord();

            foreach (var row in rows)
            {
                foreach (var measurement in row)
                {
                    csv.WriteField(measurement);
                }
                csv.NextRecord();
            }
        }

        Console.WriteLine("CSV file created successfully.");
    }
}
