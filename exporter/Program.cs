using exporter.Data;
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

string? connectionString = configuration.GetConnectionString("MyDatabase");

Console.WriteLine(connectionString);
