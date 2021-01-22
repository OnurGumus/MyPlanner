module MyPlanner.Client.View.App

open Elmish
open Elmish.Bridge
open Elmish.React
open Elmish.Navigation
open Elmish.UrlParser
open MyPlanner.Shared
open MyPlanner.Client
open MyPlanner.Client.View.Navigation

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

let inline bridgeSend msg = Bridge.Send msg

let socketEndPoint = 
    match baseUrl with
    | null
    | "" -> Constants.Socket_Endpoint
    | _ -> "/" + baseUrl + Constants.Socket_Endpoint
Program.mkProgram (Main.init newUrl Navigation.toPage bridgeSend) (Main.update bridgeSend newUrl toPage) Main.view

#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withBridgeConfig (
    Bridge.endpoint socketEndPoint
    |> Bridge.withMapping (Main.mapClientMsg) 
    |> Bridge.withWhenDown Main.ServerDisconnected
)
|> Program.toNavigable (parsePath parseRoute) (Main.urlUpdate newUrl Navigation.toPage bridgeSend)
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
