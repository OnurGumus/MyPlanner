[<RequireQualifiedAccess>]
module ElmishLoop

open System.Threading
open Elmish
open MyPlanner.Client
open MyPlanner.Server.State
open MyPlanner.Server

let run initPage
        clientModel
        (clientDispatcher: ref<Dispatch<Main.Msg>>)
        (newUrl: ref<Main.Route option -> unit>)
        (appEnv: AppEnv)
        =

    let serverDispatchQueue = ref []

    let runServerLoop serverModel dispatcher =

        let clientDispatch remoteClientMsg =
            (!) clientDispatcher (Main.mapClientMsg remoteClientMsg)

        let view model _ = serverModel := model

        let sub m =
            Cmd.ofSub (fun d ->
                dispatcher := d
                serverModel := m
                //release the buffered messages
                !serverDispatchQueue |> List.iter !dispatcher)

        Program.mkProgram (Server.init clientDispatch) (Server.update appEnv clientDispatch) view
        |> Program.withSubscription sub
        |> Program.withTrace (fun msg model -> printf "Msg: %A Model %A" msg model)
        |> Program.run

    let serverModel = ref Unchecked.defaultof<unit>

    //add messages to buffer. This implementation will be replaced by a proper dispatcher
    // once sub is invoked.
    let serverDispatcher =
        ref (fun m ->
                serverDispatchQueue
                := !serverDispatchQueue
                @ [ m ])

    let runClientLoop bridgeSend clientModel (dispatcher: ref<Dispatch<Main.Msg>>) =

        let view model _ = clientModel := model

        let sub m =
            Cmd.ofSub (fun d ->
                dispatcher := d
                clientModel := m)

        let urlUpdate = ref None

        newUrl
        := fun s ->
            urlUpdate
            := Some(fun m ->
                let (page : Main.Route option) = s
                Main.urlUpdate page m)
            //trigger update by dummy msg
            ! (dispatcher) Unchecked.defaultof<Main.Msg>

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

        Program.mkProgram (fun () -> Main.init bridgeSend initPage) update view
        |> Program.withSubscription sub
        |> Program.withTrace (fun msg model -> printf "Msg: %A Model %A" msg ())
        |> Program.withErrorHandler (fun (er, ex) ->
            (printf "\n*******\nError: %s\n Exception: %s" er (ex.ToString()))
            raise ex)
        |> Program.run

    let bridgeSend remoteMsg =
            Server.Remote remoteMsg
            |> !serverDispatcher

    runClientLoop bridgeSend clientModel clientDispatcher
    runServerLoop serverModel serverDispatcher
    //wait for initial messaging to complete.
    Thread.Sleep 1000

let dispatchHelper clientDispatcher parent msg =
    clientDispatcher (msg |> parent)
    Thread.Sleep 500

type API =
    {
        ClientModel: Main.Model ref
        ClientDispatcher: Dispatch<Main.Msg>
        NewUrl: Main.Route option -> unit
        AppEnv: AppEnv
    }


let runWithDefaults appEnv =
    let clientModel: Main.Model ref = ref Unchecked.defaultof<_>

    let initPage =
        Some(Main.Route.Tasks)

    let clientDispatcher: Dispatch<Main.Msg> ref = ref Unchecked.defaultof<_>
    let newUrl: (Main.Route option -> unit) ref = ref Unchecked.defaultof<_>
    run initPage clientModel clientDispatcher newUrl appEnv
    {
        ClientModel = clientModel
        ClientDispatcher = !clientDispatcher
        NewUrl = !newUrl
        AppEnv = appEnv
    }
