module MyPlanner.Test.CQRS.CreateTasks

open MyPlanner.Client
open TickSpec
open MyPlanner.Test
open MyPlanner.Client.Main
open Expecto
open MyPlanner.Shared.Domain

[<Given>]
let ``there are no tasks in the system`` () = Environments.AppEnv(null,[])


[<When>]
let ``I create a task`` (appEnv: Environments.AppEnv) = 

    let api = ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks)) ElmishLoop.defaultServerModel
    api.ClientDispatcher (Msg.TasksMsg(Tasks.Msg.TaskCreationRequested {Id = TaskId "1" ; Version = version0}))
    api


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` (appEnv: Environments.AppEnv,api: ElmishLoop.API) =
    let api =
        ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks)) api.ServerModel
    System.Threading.Thread.Sleep 1000
    api

[<Then>]
let ``I should see 1 task\(s\) listed`` (api: ElmishLoop.API) =
    let model = !api.ClientModel

    match model.Page.Value with
    | Page.Tasks (Some tasks) -> Expect.equal 1 tasks.Tasks.Length "incorrect task number"
    | _ -> Expect.isTrue false "not on tasks page"
