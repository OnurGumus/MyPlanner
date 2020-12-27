module MyPlanner.Client.Main

open MyPlanner.Shared.Msg
open Elmish

type Route = | Tasks

type Page = | Tasks

type ConnectionStatus =
    | Connected
    | Disconnected

type Model =
    { Page: Page option
      ConnectionStatus: ConnectionStatus }

type Msg =
    | ServerDisconnected
    | Remote of ServerToClient.Msg

let urlUpdate (result:Route option) (model: Model) =
    match result, model.Page with
    | Some (Route.Tasks), _ -> { model with Page = Some Page.Tasks }, Cmd.none
    | _ -> failwith "invalid url"

let init (bridgeSend: ClientToServer.Msg -> unit) (page: Route option) =
    urlUpdate
        page
        { Page = None
          ConnectionStatus = Disconnected }

let update (bridgeSend: ClientToServer.Msg -> unit) newUrl toPage msg model = model, Cmd.none


let mapClientMsg msg =
    match msg with
    | _ -> Remote msg
