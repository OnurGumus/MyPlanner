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

let newUrl (newUrl: string) =
    history.pushState ((), "", newUrl)
    let ev = CustomEvent.Create(NavigatedEvent)
    window.dispatchEvent ev |> ignore


let toPage =
    function
    | Route.Tasks -> "/"

let parseRoute: Parser<Route -> Route, Route> =
    let addBaseUrl p = p
        // match baseUrl with
        // | null
        // | "" -> p
        // | _ -> (s baseUrl) </> p

    oneOf [
            map (Route.Tasks) <| addBaseUrl (s "") ]
