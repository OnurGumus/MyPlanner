[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module MyPlanner.Server.Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Serilog
open Serilog.Events
open Serilog.Formatting.Compact
open MyPlanner.Server


let buildHost envFactory =

    Log.Logger <-
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Destructure.FSharpTypes()
            .WriteTo.Console(RenderedCompactJsonFormatter())
            .Enrich.FromLogContext()
            .CreateLogger()

    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder
                .UseWebRoot(Server.publicPath)
                .Configure(Action<IApplicationBuilder>(Server.configureApp envFactory))
                .ConfigureServices(Server.configureServices)
            |> ignore)
        .Build()

[<EntryPoint>]
let main _ =
    (buildHost (fun c -> Environments.AppEnv(c))).Run()
    0
