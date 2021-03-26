module MyPlanner.Client.Pages.Tasks

open Elmish
open MyPlanner.Shared.Domain
open MyPlanner.Shared.Msg

type Model = { Tasks: Task list }

type Msg =
    | Remote of ServerToClient.TasksMsg
    | TaskCreationRequested of Task

let createTaskCmd bridgeSend task =
    Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TaskCreationRequested task))

let getTasks bridgeSend =
    Cmd.ofSub
        (fun _ ->
            async {
                bridgeSend (ClientToServer.TasksRequested)
            }
            |> Async.StartImmediate)

let init bridgeSend = { Tasks = [] }, getTasks bridgeSend

let update bridgeSend msg model =
    match msg with
    | Remote (ServerToClient.TasksFetched tasks) -> { model with Tasks = tasks }, Cmd.none
    | Remote (ServerToClient.TaskCreateCompleted (Ok _)) -> model, getTasks bridgeSend
    | Remote (ServerToClient.TaskCreated t) -> { model with Tasks = t :: model.Tasks} , Cmd.none
    | TaskCreationRequested task -> model, createTaskCmd bridgeSend task
    | _ -> model, Cmd.none

let mapClientMsg msg = Remote msg
