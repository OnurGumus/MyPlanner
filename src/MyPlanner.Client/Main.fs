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

let urlUpdate result (model: Model) =
    match result, model.Page with
    | Some (Navigation.Tasks), _ -> { model with Page = Some Page.Tasks }, Cmd.none
    | _ -> failwith "invalid url"

let init (bridgeSend: ClientToServer.Msg -> unit) (page: Navigation.Page option) =
    urlUpdate
        page
        { Page = None
          ConnectionStatus = Disconnected }

let update (bridgeSend: ClientToServer.Msg -> unit) newUrl toPage msg model = model, Cmd.none
