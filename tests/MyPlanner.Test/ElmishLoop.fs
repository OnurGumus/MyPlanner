[<RequireQualifiedAccess>]
module MyPlanner.Test.ElmishLoop

open System.Threading
open Elmish
open MyPlanner.Client
open MyPlanner.Server.State
open MyPlanner.Server
open MyPlanner.Shared.Domain

let defaultServerModel = ref Unchecked.defaultof<Task list>

let run
    initPage
    clientModel
    serverModel
    (clientDispatcher: ref<Dispatch<Main.Msg>>)
    (newUrl: ref<Main.Route option * bool -> unit>)
    (appEnv: MyPlanner.Test.Environments.AppEnv)
    =

    let serverDispatchQueue = ref []

    let runServerLoop serverModel dispatcher =

        let clientDispatch remoteClientMsg =
            (!) clientDispatcher (Main.mapClientMsg remoteClientMsg)

        let view model _ = serverModel := model

        let sub m =
            Cmd.ofSub
                (fun d ->
                    dispatcher := d
                    serverModel := m
                    //release the buffered messages
                    !serverDispatchQueue |> List.iter !dispatcher)

        Program.mkProgram (State.init clientDispatch) (State.update appEnv clientDispatch) view
        |> Program.withSubscription sub
        |> Program.withTrace (fun msg model -> printfn "Server-Msg: %A \nServer-Model %A" msg model)
        |> Program.run



    //add messages to buffer. This implementation will be replaced by a proper dispatcher
    // once sub is invoked.
    let serverDispatcher =
        ref
            (fun m ->
                serverDispatchQueue
                := !serverDispatchQueue @ [ m ])

    let runClientLoop bridgeSend clientModel (dispatcher: ref<Dispatch<Main.Msg>>) =

        let view model _ = clientModel := model

        let sub m =
            Cmd.ofSub
                (fun d ->
                    dispatcher := d
                    clientModel := m)

        let urlUpdate = ref None

        newUrl
        := fun (s, _) ->
            urlUpdate
            := Some
                (fun m ->
                    let (page: Main.Route option) = s
                    Main.urlUpdate !newUrl Some bridgeSend page m)
            //trigger update by dummy msg
            let d = !dispatcher

            d Unchecked.defaultof<Main.Msg>

        let update (msg: Main.Msg) (model: Main.Model): Main.Model * Cmd<Main.Msg> =
            clientModel := model

            match !urlUpdate with
            | Some f ->
                urlUpdate := None
                f model
            | _ ->
                let model, cmd =
                    Main.update bridgeSend (!newUrl) Some msg model

                clientModel := model
                model, cmd

        Program.mkProgram (fun () -> Main.init !newUrl Some bridgeSend initPage) update view
        |> Program.withSubscription sub
        |> Program.withTrace
            (fun msg model -> System.Console.WriteLine("Client-Msg: {0} \nClient-Model {1}", msg, model)) //somehow printf crashes the compiler
        |> Program.withErrorHandler
            (fun (er, ex) ->
                (printfn "\n*******\nError: %s\n Exception: %s" er (ex.ToString()))
                raise ex)
        |> Program.run

    let bridgeSend remoteMsg =
        State.Remote remoteMsg |> !serverDispatcher

    runClientLoop bridgeSend clientModel clientDispatcher
    runServerLoop serverModel serverDispatcher
    //wait for initial messaging to complete.
    Thread.Sleep 1000

let dispatchHelper clientDispatcher parent msg =
    clientDispatcher (msg |> parent)
    Thread.Sleep 500

type API =
    { ClientModel: Main.Model ref
      ServerModel: Task list ref
      ClientDispatcher: Dispatch<Main.Msg>
      NewUrl: Main.Route option * bool -> unit
      AppEnv: MyPlanner.Test.Environments.AppEnv }


let runWithDefaults (appEnv: MyPlanner.Test.Environments.AppEnv) initPage serverModel =
    let clientModel: Main.Model ref = ref Unchecked.defaultof<_>

    let clientDispatcher: Dispatch<Main.Msg> ref = ref Unchecked.defaultof<_>
    let newUrl: (Main.Route option * bool -> unit) ref = ref Unchecked.defaultof<_>
    run initPage clientModel serverModel clientDispatcher newUrl appEnv

    { ClientModel = clientModel
      ClientDispatcher = !clientDispatcher
      ServerModel = serverModel
      NewUrl = !newUrl
      AppEnv = appEnv }
