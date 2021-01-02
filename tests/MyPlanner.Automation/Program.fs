module MyPlanner.Automation.Program

open System.Reflection
open TickSpec
open canopy.classic
open canopy.configuration
open canopy.types


[<BeforeScenarioAttribute>]
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

[<AfterScenarioAttribute>]
let quitBrowser () = quit ()

[<EntryPointAttribute>]
let main _ =
    try
        do
            use host =
                (MyPlanner.Server.Program.buildHost (fun _ -> MyPlanner.Test.Environments.AppEnv()))

            host.StartAsync(Unchecked.defaultof<_>) |> ignore
            let ass = Assembly.GetExecutingAssembly()
            let definitions = StepDefinitions(ass)

            [ "create-tasks" ]
            |> Seq.iter
                (fun source ->
                    let s =
                        ass.GetManifestResourceStream("MyPlanner.Automation." + source + ".feature")

                    definitions.Execute(source, s))

        0
    with e ->
        printf "%A" e
        -1
