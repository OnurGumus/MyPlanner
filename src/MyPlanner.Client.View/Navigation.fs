module MyPlanner.Client.View.Navigation

open Elmish.UrlParser
open System

open Fable.Core
open Browser
open Browser.Types
open Elmish
open MyPlanner.Client.Main

[<Global>]
let baseUrl: string = jsNative

[<Literal>]
let internal NavigatedEvent = "NavigatedEvent"

let newUrl (newUrl: string, replaceState) =
    if replaceState then
        history.replaceState (null, "", newUrl)
    else
        history.pushState (null, "", newUrl)

    let ev = CustomEvent.Create(NavigatedEvent)
    window.dispatchEvent ev |> ignore


let toPage =
    function
    | Route.Tasks -> "tasks"

let parseRoute: Parser<Route -> Route, Route> =
    let addBaseUrl p =
        match baseUrl with
        | null
        | "" -> p
        | _ -> (s baseUrl) </> p

    oneOf [ map (Route.Tasks) <| addBaseUrl (s "")
            map (Route.Tasks) <| addBaseUrl (s "tasks") ]
