module MyPlanner.Test.CQRS.CreateTasks

open MyPlanner.Client
open TickSpec
open MyPlanner.Test
open MyPlanner.Client.Main
open Expecto
open MyPlanner.Shared.Domain
open MyPlanner.Client.Pages

[<Given>]
let ``there are no tasks in the system`` () = Environments.AppEnv(null, [])


[<When>]
let ``I create a task`` (appEnv: Environments.AppEnv) =

    let api =
        ElmishLoop.runWithDefaults appEnv (Some(Main.Route.Tasks)) ElmishLoop.defaultServerModel

    let t =
        { Id = "1" |> ShortString.ofString |> TaskId
          Version = version0
          Title = TaskTitle(ShortString.ofString "title")
          Description = TaskDescription(LongString.ofString "desc") }

    api.ClientDispatcher(Msg.TasksMsg(Tasks.Msg.TaskCreationRequested t))
    api


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` (appEnv: Environments.AppEnv, api: ElmishLoop.API) =
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
