module MyPlanner.Automation.StartPage
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open MyPlanner.Shared.Domain

[<Given>]
let ``there is 1 task on the system`` () = 
    let tasks =
        [ {   Id = "test_task" |> ShortString.ofString |> TaskId
              Version = version0
              Title = TaskTitle(ShortString.ofString "title")
              Description = TaskDescription(LongString.ofString "desc") }]
    AppEnv(Host.config,tasks)


[<When>]
let ``I visit the start page`` ((appEnv: AppEnv)) = 
    Host.startHost (appEnv)
    Host.startBrowser()
    url "http://localhost:8085"

[<Then>]
let ``I should be redirect to /tasks`` () = 
    sleep 5
    currentUrl() |> contains "tasks"
