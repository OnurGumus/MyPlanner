module MyPlanner.Server.State

open Query
open MyPlanner.Shared.Msg
open MyPlanner.Shared.Domain
open Elmish

let cmdOfSub v = Cmd.ofSub (fun _ -> v)
module Tasks =
    type Msg =
        | TasksFetched of Task list
        | TasksMsg of ClientToServer.TasksMsg

    let fetchTasksCmd (env: #IQuery) =
        Cmd.OfAsync.perform env.Query<Task> () TasksFetched

    let update (env: #IQuery) clientDispatch (msg: Msg) (state: Task list) =
        match msg with
        | TasksMsg (ClientToServer.TasksRequested) -> state, fetchTasksCmd env

        | TasksFetched tasks ->
            state,
            tasks
            |> ServerToClient.TasksFetched
            |> clientDispatch
            |> cmdOfSub

type ServerMsg =
    | Remote of ClientToServer.Msg
    | TasksMsg of Tasks.Msg


let init dispatch () =
    dispatch ServerToClient.ServerConnected
    ([]: Task list), Cmd.none

let rec update env clientDispatch msg state =
    match msg, state with
    | TasksMsg m, _ ->
        let state, cmd =
            Tasks.update env (ServerToClient.TasksMsg >> clientDispatch) m state

        state, Cmd.map TasksMsg cmd

    | Remote (ClientToServer.TasksMsg tasksMsg), _ ->
        //client to server message transformation
        let msg =
            tasksMsg |> Tasks.Msg.TasksMsg |> TasksMsg
        //recurse
        update env clientDispatch msg state



