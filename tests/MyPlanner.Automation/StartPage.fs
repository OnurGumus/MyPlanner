module MyPlanner.Automation.StartPage
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open MyPlanner.Shared.Domain

[<Given>]
let ``there is 1 task on the system`` () = 
    let tasks =
        [ { Id = TaskId "1"; Version = version0 } ]
    AppEnv([])


[<When>]
let ``I visit the start page`` ((appEnv: AppEnv)) = 
    Host.startHost (AppEnv([]))
    Host.startBrowser()
    url "http://localhost:5000"

[<Then>]
let ``I should be redirect to /tasks`` () = ()
