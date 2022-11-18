 
//### $Signal$ $Analysis$

//This project main strategy consists of shorting stocks with a high price and going long on those with low prices. 
//This strategy is supported by the idea that stocks with low prices will generate higher returns to compensate for the higher degree of risk that usually underperforming stocks have.
// The price per Share (Prc) signal is going to be use as the predictor of future returns. It is constructed as the absolute value of the price or bid/ask average and is classified as a size factor


#r "nuget: FSharp.Data"
#r "nuget: FSharp.Stats"
#r "nuget: Plotly.NET,2.0.0-preview.17"
#r "nuget: Plotly.NET.Interactive,2.0.0-preview.17"

open System
open FSharp.Data
open FSharp.Stats
open Plotly.NET 

// Set dotnet interactive formatter to plaintext
Formatter.Register(fun (x:obj) (writer: TextWriter) -> fprintfn writer "%120A" x )
Formatter.SetPreferredMimeTypesFor(typeof<obj>, "text/plain")
// Make plotly graphs work with interactive plaintext formatter
Formatter.SetPreferredMimeTypesFor(typeof<GenericChart.GenericChart>,"text/html")

let [<Literal>] ResolutionFolder = __SOURCE_DIRECTORY__
Environment.CurrentDirectory <- ResolutionFolder

#load "Portfolio.fsx"
open Portfolio
#load "Common.fsx"
open Common

#load "YahooFinance.fsx"
open YahooFinance

#r "nuget: DiffSharp-lite"
open DiffSharp

let [<Literal>] IdAndReturnsFilePath = "data/id_and_return_data.csv"
let [<Literal>] MySignalFilePath = "data/prc.csv"
let strategyName = "Price-per-Share"
let ff3 = French.getFF3 Frequency.Monthly

type IdAndReturnsType = 
    CsvProvider<Sample=IdAndReturnsFilePath,
                Schema="obsMain(string)->obsMain=bool,exchMain(string)->exchMain=bool",
                ResolutionFolder=ResolutionFolder>

type MySignalType = 
    CsvProvider<MySignalFilePath,
                ResolutionFolder=ResolutionFolder>
                
let idAndReturnsCsv = IdAndReturnsType.GetSample()
let mySignalCsv = MySignalType.GetSample()

let idAndReturnsRows = idAndReturnsCsv.Rows |> Seq.toList
let mySignalRows = mySignalCsv.Rows |> Seq.toList

// Number of stocks per month in the signal data 
let countMySignalRows (rows: list<MySignalType.Row>) =
    let byMonth =
        rows
        |> List.groupBy (fun row -> row.Eom)
        |> List.sortBy (fun (month, rows) -> month)
    [ for (month, rows) in byMonth do
        let nStocks = 
            rows
            |> List.map (fun row -> row.Id)
            |> List.distinct
            |> List.length
        month, nStocks ]

mySignalRows
|> countMySignalRows
|> List.truncate 10

mySignalRows
|> countMySignalRows
|> Chart.Column
|> Chart.withXAxisStyle (TitleText="Month")
|> Chart.withYAxisStyle (TitleText="Number of Stocks")
|> Chart.withTitle("Number of stocks per month in the signal data")
|> Chart.withTemplate ChartTemplates.lightMirrored
|> Chart.withSize(400.,400.)