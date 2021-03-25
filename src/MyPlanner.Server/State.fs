module MyPlanner.Server.State

open Query
open MyPlanner.Shared.Msg
open MyPlanner.Shared.Domain
open Elmish
open Command
open Akkling.Streams
open Akka.Streams

let cmdOfSub v = Cmd.ofSub (fun _ -> v)



module Tasks =
    type Model = Task list

    type Msg =
        | TasksFetched of Task list
        | TaskCreateCompleted of Result<Task, string>
        | TasksMsg of ClientToServer.TasksMsg
        | SubscribedToStream of IKillSwitch
        | DataEventOccurred of MyPlanner.Query.Projection.DataEvent


    let subscribeCmd (env: #IQuery) =
        Cmd.ofSub(fun dispatcher ->
            let ks  = env.Subscribe ((fun event -> dispatcher (DataEventOccurred event)))
            dispatcher (SubscribedToStream ks))

    let fetchTasksCmd (env: #IQuery) =
        Cmd.OfAsync.perform env.Query<Task> () TasksFetched

    let creteTask (env: #ITaskCommand) task =
        Cmd.OfAsync.perform env.CreateTask task TaskCreateCompleted

    let update env clientDispatch (msg: Msg) (state: Model) =
        match msg with
        | TasksMsg (ClientToServer.TasksRequested) ->
            let q = subscribeCmd env
            let f = fetchTasksCmd env
            state, Cmd.batch [ q; f ]
        | TasksMsg (ClientToServer.TaskCreationRequested task) -> state, creteTask env task
        | DataEventOccurred (MyPlanner.Query.Projection.TaskEvent (MyPlanner.Query.Projection.TaskCreated task)) ->
            state,
            task
            |> ServerToClient.TaskCreated
            |> clientDispatch
            |> cmdOfSub
        | TaskCreateCompleted task ->
            state,
            task
            |> ServerToClient.TaskCreateCompleted
            |> clientDispatch
            |> cmdOfSub
        | TasksFetched tasks ->
            state,
            tasks
            |> ServerToClient.TasksFetched
            |> clientDispatch
            |> cmdOfSub
        | _ -> state, Cmd.none

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
