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
    open MyPlanner.Query.Projection

    type Mode =
        | Acumulating
        | Streaming

    type Model =
        {
            Tasks: Task list
            KillSwitch: IKillSwitch option
            AccumulatedEvents: TaskEvent list
            Mode: Mode
        }

    type Msg =
        | TasksFetched of Task list
        | TaskCreateCompleted of Result<Task, string>
        | TasksMsg of ClientToServer.TasksMsg
        | SubscribedToStream of IKillSwitch
        | DataEventOccurred of MyPlanner.Query.Projection.DataEvent


    let subscribeCmd (env: #IQuery) =
        Cmd.ofSub
            (fun dispatcher ->
                let ks =
                    env.Subscribe((fun event -> dispatcher (DataEventOccurred event)))

                dispatcher (SubscribedToStream ks))

    let fetchTasksCmd (env: #IQuery) =
        Cmd.OfAsync.perform env.Query<Task> () TasksFetched

    let creteTask (env: #ITaskCommand) task =
        Cmd.OfAsync.perform env.CreateTask task TaskCreateCompleted

    let init =
        {
            Tasks = []
            KillSwitch = None
            AccumulatedEvents = []
            Mode = Acumulating
        },
        Cmd.none

    let update env clientDispatch (msg: Msg) (state: Model) =
        match msg, state with

        | TasksMsg (ClientToServer.TasksRequested), { Mode = Streaming } ->
            { state with Mode = Acumulating }, fetchTasksCmd env

        | TasksMsg (ClientToServer.TasksRequested), _ ->  state, subscribeCmd env

        | TasksMsg (ClientToServer.TaskCreationRequested task), _ -> state, creteTask env task

        | DataEventOccurred (TaskEvent event), { Mode = Acumulating } ->
            { state with
                AccumulatedEvents = state.AccumulatedEvents @ [ event ]
            },
            Cmd.none
        | DataEventOccurred (TaskEvent (TaskCreated task)), _ ->
            state,
            task
            |> ServerToClient.TaskCreated
            |> clientDispatch
            |> cmdOfSub

        | TaskCreateCompleted task, _ ->
            state,
            task
            |> ServerToClient.TaskCreateCompleted
            |> clientDispatch
            |> cmdOfSub

        | TasksFetched tasks, _ ->
            let tasks =
                [
                    for (TaskCreated t) in state.AccumulatedEvents do
                        yield t
                    yield! tasks
                ]

            { state with
                Tasks = tasks
                Mode = Streaming
                AccumulatedEvents = []
            },
            tasks
            |> ServerToClient.TasksFetched
            |> clientDispatch
            |> cmdOfSub

        | SubscribedToStream ks, _ -> { state with KillSwitch = Some ks }, fetchTasksCmd env

    let dispose (state: Model) =
        match state.KillSwitch with
        | Some ks -> state, Cmd.ofSub (fun _ -> ks.Shutdown())
        | _ -> state, Cmd.none


type ServerMsg =
    | Remote of ClientToServer.Msg
    | TasksMsg of Tasks.Msg

type Model = Tasks of Tasks.Model

let init dispatch () =
    dispatch ServerToClient.ServerConnected
    let tasksModel, cmd = Tasks.init
    (Tasks tasksModel), Cmd.map TasksMsg cmd

let rec update env clientDispatch msg (state : Model) =
    match msg, state with
    | TasksMsg m, Tasks state ->
        let state, cmd =
            Tasks.update env (ServerToClient.TasksMsg >> clientDispatch) m state

        Tasks state, Cmd.map TasksMsg cmd

    | Remote (ClientToServer.TasksMsg tasksMsg), _ ->
        //client to server message transformation
        let msg =
            tasksMsg |> Tasks.Msg.TasksMsg |> TasksMsg
        //recurse
        update env clientDispatch msg state
