namespace MyPlanner.Client.Main

open MyPlanner.Shared.Msg

type Route = | Tasks

type Page = | Tasks

type ConnectionStatus =
    | Connected
    | Disconnected

type Model =
    { Page: Page
      ConnectionStatus: ConnectionStatus }

type Msg =
    | ServerDisconnected
    | Remote of ServerToClient.Msg
