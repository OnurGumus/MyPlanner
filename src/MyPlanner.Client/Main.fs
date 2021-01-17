module MyPlanner.Client.Main

open MyPlanner.Shared.Msg
open Elmish

type Route = Tasks

type Page = Tasks of Tasks.Model option

type ConnectionStatus =
    | Connected
    | Disconnected

type Model =
    { Page: Page option
      ConnectionStatus: ConnectionStatus }

type Msg =
    | ServerDisconnected
    | Remote of ServerToClient.Msg
    | TasksMsg of Tasks.Msg

let startPage = Route.Tasks |> Some

let initTask model =
    let tasksModel, tasksCmd = Tasks.init

    { model with
          Page = tasksModel |> Some |> Tasks |> Some },
    Cmd.map TasksMsg tasksCmd

//called externally when url changed from the code at runtime.
//not called when you land to a page if you type it to the address bar
let urlUpdate (result: Route option) (model: Model) =
    match result, model.Page with
    | Some (Route.Tasks), Some _ -> initTask model
    //initial landing handled below
    | Some (Route.Tasks), None -> { model with Page = Some(Tasks(None)) }, Cmd.none
    | _ -> failwith "invalid url"

let init (bridgeSend: ClientToServer.Msg -> unit) (page: Route option) =
    urlUpdate
        page
        { Page = None
          ConnectionStatus = Disconnected }

let handleConnect model =
    match model.Page with
    | Some (Page.Tasks _) -> initTask model
    | _ -> model, Cmd.none

let update (bridgeSend: ClientToServer.Msg -> unit) newUrl toPage msg model =
    match msg, model with
    | Remote (ServerToClient.ServerConnected), _ ->
        let model =
            { model with
                  ConnectionStatus = Connected }
        //we postpone the actual init of page till connection established
        match model.Page.Value with
        | (Tasks None) -> initTask model
        | _ -> model, Cmd.none //page is already init
    | ServerDisconnected, _ ->
        { model with
              ConnectionStatus = Disconnected },
        Cmd.none

    | TasksMsg msg, { Page = Some (Tasks tasksModel) } ->
        let newModel, cmd = Tasks.update msg tasksModel

        { model with
              Page = Some(Tasks newModel) },
        Cmd.map TasksMsg cmd

    | msg, model ->
        invalidOp
        <| sprintf "not supported case %A %A" msg model



let mapClientMsg msg =
    match msg with
    | _ -> Remote msg
