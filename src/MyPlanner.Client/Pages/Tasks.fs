module MyPlanner.Client.Pages.Tasks

open Elmish
open MyPlanner.Shared.Domain
open MyPlanner.Shared.Msg

type DialogStatus =
    | Open
    | Closed

type Model =
    {
        Tasks: Task list
        DialogStatus: DialogStatus
    }

type Msg =
    | Remote of ServerToClient.TasksMsg
    | TaskCreationRequested of Task
    | DialogOpened
    | DialogClosed

let createTaskCmd bridgeSend task =
    Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TaskCreationRequested task))

let init bridgeSend =
    { Tasks = []; DialogStatus = Closed }, Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TasksRequested))

let update bridgeSend msg model =
    match msg with
    | DialogOpened -> { model with DialogStatus = Open }, Cmd.none
    | DialogClosed -> { model with DialogStatus = Closed }, Cmd.none
    | Remote (ServerToClient.TasksFetched tasks) -> { model with Tasks = tasks }, Cmd.none
    | TaskCreationRequested task -> model, createTaskCmd bridgeSend task
    | _ -> model, Cmd.none

let mapClientMsg msg = Remote msg
