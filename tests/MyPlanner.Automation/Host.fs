module MyPlanner.Automation.Host
open System.Reflection
open TickSpec
open canopy.configuration
open canopy.types

open canopy.classic


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
    let host = (MyPlanner.Server.Program.buildHost (fun _ -> appEnv))
    host.StartAsync(Unchecked.defaultof<_>) |> ignore
           
        
