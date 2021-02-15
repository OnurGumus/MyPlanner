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

open Microsoft.Extensions.Configuration
open Hocon.Extensions.Configuration
open MyPlanner.Shared
let clientPath  =
#if DEBUG 
   [(Constants.ClientPath,"../MyPlanner.Client.View/public")]  |> dict
#else
    [(Constants.ClientPath,"./clientFiles")] |> dict
#endif
let configBuilder =
    ConfigurationBuilder()
        .AddHoconFile(Constants.ConfigHocon)
        .AddEnvironmentVariables()
        .AddInMemoryCollection(clientPath)



let buildHost (configBuilder: IConfigurationBuilder) envFactory =
    let config = configBuilder.Build()

    Log.Logger <-
        LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Destructure.FSharpTypes()
            .WriteTo.Console(RenderedCompactJsonFormatter())
            .Enrich.FromLogContext()
            .CreateLogger()

    let publicPath = Server.publicPath config

    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webBuilder ->
            webBuilder
                .UseWebRoot(publicPath)
                .UseContentRoot(publicPath)
                .Configure(Action<IApplicationBuilder>(Server.configureApp config envFactory))
                .ConfigureServices(Server.configureServices)
                .UseUrls("http://0.0.0.0:8085/")
            |> ignore)
        .Build()

[<EntryPoint>]
let main _ =
    (buildHost configBuilder (fun c -> Environments.AppEnv(c)))
        .Run()

    0
