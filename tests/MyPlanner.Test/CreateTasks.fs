module MyPlanner.Test.CQRS.CreateTasks

open MyPlanner.Client
open TickSpec
open MyPlanner.Test
open MyPlanner.Client.Main
open Expecto

[<Given>]
let ``there are no tasks in the system`` () = Environments.AppEnv([])


[<When>]
let ``I create a task`` () = ()


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` (appEnv: Environments.AppEnv) =
    let api =
        ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks))

    System.Threading.Thread.Sleep 1000
    api

[<Then>]
let ``I should see 1 task\(s\) listed`` (api: ElmishLoop.API) =
    let model = !api.ClientModel

    match model.Page.Value with
    | Page.Tasks (Some tasks) -> Expect.equal 1 tasks.Tasks.Length "incorrect task number"
    | _ -> Expect.isTrue false "not on tasks page"
