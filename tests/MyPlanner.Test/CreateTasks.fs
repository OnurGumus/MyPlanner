module MyPlanner.Test.CQRS.CreateTasks

open MyPlanner.Client
open TickSpec
open MyPlanner.Test

[<Given>]
let ``there are no tasks in the system`` () = ()


[<When>]
let ``I create a task`` () = (Environments.AppEnv())


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` (appEnv: Environments.AppEnv) =
    ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks))
    |> ignore

    System.Threading.Thread.Sleep 1000

[<Then>]
let ``I should see 1 task\(s\) listed`` () = ()
