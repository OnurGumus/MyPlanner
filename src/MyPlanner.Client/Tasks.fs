module MyPlanner.Client.Tasks

open Elmish
open MyPlanner.Shared.Domain
open MyPlanner.Shared.Msg

type Model = { Tasks: Task list }

type Msg = Remote of ServerToClient.TasksMsg

let init bridgeSend =
    { Tasks = [] }, Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TasksRequested))

let update msg model =
    match msg with
    | Remote (ServerToClient.TasksFetched tasks) -> { model with Tasks = tasks }, Cmd.none

let mapClientMsg msg = Remote msg
