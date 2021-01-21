module MyPlanner.Client.Tasks

open Elmish
open MyPlanner.Shared.Domain
open MyPlanner.Shared.Msg

type Model = { Tasks: Task list }

type Msg = 
    | Remote of ServerToClient.TasksMsg
    | TaskCreationRequested of Task
   
let createTaskCmd bridgeSend task = 
    Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TaskCreationRequested task))

let init bridgeSend =
    { Tasks = [] }, Cmd.ofSub (fun _ -> bridgeSend (ClientToServer.TasksRequested))

let update bridgeSend msg model =
    match msg with
    | Remote (ServerToClient.TasksFetched tasks) -> { model with Tasks = tasks }, Cmd.none
    | TaskCreationRequested task ->  model, createTaskCmd  bridgeSend task
    | _ -> model, Cmd.none

let mapClientMsg msg = Remote msg
