module MyPlanner.Client.Pages.Signin

open Elmish
open MyPlanner.Shared.Domain
open MyPlanner.Shared.Msg

type Model = NAModel

type Msg = NA


let init () = NAModel, Cmd.none

let update msg model =
    match msg with
    | _ -> model, Cmd.none

