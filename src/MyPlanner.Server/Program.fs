open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Serilog
open Serilog.Events
open Serilog.Formatting.Compact
open MyPlanner.Server


[<EntryPoint>]
let main _ =
    Log.Logger <-
        LoggerConfiguration().MinimumLevel.Debug().MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Destructure.FSharpTypes()
            .WriteTo.Console(RenderedCompactJsonFormatter()).Enrich.FromLogContext().CreateLogger()

    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder.UseWebRoot(Server.publicPath)
                      .Configure(Action<IApplicationBuilder> Server.configureApp)
                      .ConfigureServices(Server.configureServices)
            |> ignore)
        .Build()
        .Run()
    0