# fp-data-exporter
Helper project for easy Goslar flood prediction data export for analysis and AI model training.

## Prerequisites

This program was developed specifically for the API for a project on floodprediction by the TU Clausthal. Therefore, it requires data collected by said API.

Other than that, a way to install dependencies as listed in the .csproj file, build and refer to usage for instructions on extracting data from a running database.

## Usage

The following example expects a ms sql local db server running in the background.

```
exporter.exe "Server=(LocalDb)\MSSQLLocalDB;Database=FloodpredictionDB;ApplicationIntent=ReadOnly;Trusted_Connection=true;" --from 2023-01-01 --to 2023-01-03T23:59 --sensors 5000:10 5000:130 --format Csv --output export.csv
```

Setting format to Csv exports in comma seperated value format with colums being firstly a column for timestamps and the rest sensor domain ids. Therefore rows are all requested sensor measurements for each individual (15 minute interval) timestamp.

Other available formats are Summary (to see how many datasets exist for each sensor) and Preview, showing the command output in the CLI (choose a smaller time interval for this).

Available sensors can be found in the Architecture repository in the internal TU Clausthal gitlab.

# TODO

Currently, the user has to export the data from the live system onto their own machine to have access, but in the future it would be convenient to have an endpoint in the API serving the same functionality as this console program.
