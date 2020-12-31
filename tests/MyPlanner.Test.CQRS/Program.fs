module Program

open Expecto.Tests
open Serilog
open Serilog.Formatting.Compact


[<EntryPoint>]
let main args =
    Log.Logger <-
        LoggerConfiguration().MinimumLevel.Debug()
            .Destructure.FSharpTypes()
            .WriteTo.Console(RenderedCompactJsonFormatter()).Enrich.FromLogContext().CreateLogger()

    runTestsInAssemblyWithCLIArgs [] [|
        "--sequenced"
    |]
