# fp-data-exporter
Helper project for easy Goslar flood prediction data export for analysis and AI model training.

## Prerequisites

This program was developed specifically for the API for a project on floodprediction by the TU Clausthal. Therefore, it requires data collected by said API.

Other than that, a way to install dependencies as listed in the .csproj file, build and refer to usage for instructions on extracting data from a running database.

## Usage

The following example expects a ms sql local db server running in the background.

```
exporter.exe "Server=(LocalDb)\MSSQLLocalDB;Database=FloodpredictionDB;ApplicationIntent=readonly;" --from 2023-01-01 --to 2023-01-03T23:59 --sensors 5000:10 5000:130 --format Csv --output export.csv
```
