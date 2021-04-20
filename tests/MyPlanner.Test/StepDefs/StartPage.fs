module MyPlanner.Test.StepDefs.StartPage

open MyPlanner.Client
open TickSpec
open MyPlanner.Test
open MyPlanner.Shared.Domain
open MyPlanner.Test.Environments
open Expecto
open MyPlanner.Client.Main


[<Given>]
let ``I am not logged in`` () =

    Environments.AppEnv(null)

[<When>]
let ``I visit the start page`` (appEnv: AppEnv) =
    let api =
        ElmishLoop.runWithDefaults appEnv Main.startPage ElmishLoop.defaultServerModel

    System.Threading.Thread.Sleep 500
    api

[<When>]
let ``I visit the signin page`` (appEnv: AppEnv) =
    let api =
        ElmishLoop.runWithDefaults appEnv (Route.Signin |> Some) ElmishLoop.defaultServerModel

    System.Threading.Thread.Sleep 500
    api

[<Then>]
let ``I should be at the signin page`` (api: ElmishLoop.API) =
    let model = !api.ClientModel

    match model.ConnectionStatus, model.Page with
    | Main.Connected, Some (Main.Page.Signin (_)) -> true
    | _ -> false
    |> Expect.isTrue
    <| "Not on the sign-in page"

[<When>]
let ``I click to the signup link`` (api: ElmishLoop.API) =
    api.NewUrl (Some Route.Signup, true)

[<Then>]
let ``I should be at the signup page`` (api: ElmishLoop.API) =
    let model = !api.ClientModel

    match model.ConnectionStatus, model.Page with
    | Main.Connected, Some (Main.Page.Signup (_)) -> true
    | _ -> false
    |> Expect.isTrue
    <| "Not on the sign-up page"


 
    
