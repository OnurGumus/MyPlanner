module MyPlanner.Automation.Host
open TickSpec
open canopy.configuration
open canopy.types

open canopy.classic
open MyPlanner
open Microsoft.Extensions.Configuration
open MyPlanner.Shared
open Microsoft.Extensions.Hosting

let clientPath  = [(Constants.ClientPath,"../../deploy/clientFiles")] |> dict
let configBuilder = 
        Server.Program.configBuilder
            .AddInMemoryCollection(clientPath)

let config = configBuilder.Build()

let mutable host : IHost = Unchecked.defaultof<_>

let stopHost () =
    if host <> null then 
        host.Dispose()

let startBrowser () =
    chromiumDir <- System.AppContext.BaseDirectory
    let options = OpenQA.Selenium.Chrome.ChromeOptions()
    options.AddArgument("--disable-extensions")
    options.AddArgument("disable-infobars")
    options.AddArgument("test-type")
    options.AddArgument("--headless")
    options.AddArgument("--no-sandbox")
    options.AddArgument("--disable-gpu")
    options.AddArgument("--disable-setuid-sandbox")
    options.AddArgument("--whitelisted-ips")
    options.AddArgument("--disable-dev-shm-usage")
    options.AddUserProfilePreference("download.default_directory", ".")
    start <| ChromiumWithOptions options
    positionBrowser 0 0 60 200

let startHost appEnv =
    host <- (Server.Program.buildHost configBuilder (fun _ -> appEnv))
    host.StartAsync(Unchecked.defaultof<_>) |> ignore
    
           
        
