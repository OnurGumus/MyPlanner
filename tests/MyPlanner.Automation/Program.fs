module MyPlanner.Automation.Program

open System.Reflection
open TickSpec
open canopy.classic

[<AfterScenarioAttribute>]
let quitBrowser () = 
    Host.stopHost()
    quit ()

[<EntryPointAttribute>]
let main _ = 
    try
        do
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
