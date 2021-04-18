module MyPlanner.Client.View.Pages.Signin

open MyPlanner.Client
open Pages.Signin
open MyPlanner.Shared.Domain
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Dom
open MyPlanner.Client.View
open System
open Browser.Types
open Fable
open MyPlanner.Client.View.Util

ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_wwwroot/design/signin/index.html")

let layoutCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/layout.css")


Browser.Dom.document?adoptedStyleSheets <- [| layoutCSS |] |> Array.map createSheet

[<ReactComponent>]
let SigninView dispatch (model: Model) =

    let attachShadowRoot, shadowRoot = Util.useShadowRoot (html)

    React.useLayoutEffect (
        (fun _ ->
            match shadowRoot with
            | Some shadowRoot -> shadowRoot?adoptedStyleSheets <- [| layoutCSS |] |> Array.map createSheet
            | _ -> ()
            ),
        [| shadowRoot |> box<_> |]
    )

    Interop.createElement
        "signin-page"
        [
            attachShadowRoot
            prop.className "page"
            prop.children []
        ]
