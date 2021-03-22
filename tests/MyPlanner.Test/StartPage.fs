module MyPlanner.Test.CQRS.StartPage

open MyPlanner.Client
open TickSpec
open MyPlanner.Test
open MyPlanner.Shared.Domain
open MyPlanner.Test.Environments
open Expecto

[<Given>]
let ``there is 1 task on the system`` () =
    let tasks =
        [   {       Id = "1" |> ShortString.ofString |> TaskId
                    Version = version0; 
                    Title = TaskTitle (ShortString.ofString "title"); 
                    Description = TaskDescription (LongString.ofString "desc") } ]

    Environments.AppEnv(null,tasks)

[<When>]
let ``I visit the start page`` (appEnv: AppEnv) =
    let api =
        ElmishLoop.runWithDefaults appEnv Main.startPage ElmishLoop.defaultServerModel

    System.Threading.Thread.Sleep 500
    api

[<Then>]
let ``I should be redirect to /tasks`` (api: ElmishLoop.API) =
    let model = !api.ClientModel

    match model.ConnectionStatus, model.Page with
    | Main.Connected, Some (Main.Page.Tasks (Some _)) -> true
    | _ -> false
    |> Expect.isTrue
    <| "Not on the tasks page"
