module MyPlanner.Test.CQRS.CreateTasks

open MyPlanner.Client
open TickSpec
open MyPlanner.Test

[<Given>]
let ``there are no tasks in the system`` () = Environments.AppEnv([])


[<When>]
let ``I create a task`` () = ()


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` (appEnv: Environments.AppEnv) =
    let api = ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks))
    System.Threading.Thread.Sleep 1000
    api

[<Then>]
let ``I should see 1 task\(s\) listed`` (api:ElmishLoop.API) = 
   // let model = !api.ClientModel
    ()
