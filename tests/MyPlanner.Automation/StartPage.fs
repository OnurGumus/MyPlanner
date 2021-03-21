module MyPlanner.Automation.StartPage
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open MyPlanner.Shared.Domain

[<Given>]
let ``there is 1 task on the system`` () = 
    let tasks =
        [ {   Id = "test_task" |> ShortString |> TaskId
              Version = version0
              Title = TaskTitle(ShortString "title")
              Description = TaskDescription(LongString "desc") }]
    AppEnv(Host.config,[])


[<When>]
let ``I visit the start page`` ((appEnv: AppEnv)) = 
    Host.startHost (AppEnv(Host.config,[]))
    Host.startBrowser()
    url "http://localhost:8085"

[<Then>]
let ``I should be redirect to /tasks`` () = 
    sleep 5
    currentUrl() |> contains "tasks"
