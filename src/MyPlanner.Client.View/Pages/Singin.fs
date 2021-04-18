module MyPlanner.Client.View.Pages.Signin

open MyPlanner.Client
open Pages.Signin
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open MyPlanner.Client.View
open Fable
open MyPlanner.Client.View.Util

ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_wwwroot/_signin/index.html")

let mainLayoutCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/layout.css")

let mainTextCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/text.css")

let layoutCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/_signin/layout.css")

let appearanceCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/_signin/appearance.css")

let textCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/_signin/text.css")



[<ReactComponent>]
let SigninView dispatch (model: Model) =

    let attachShadowRoot, shadowRoot = Util.useShadowRoot (html)

    React.useLayoutEffect (
        (fun _ ->
            match shadowRoot with
            | Some shadowRoot ->
                shadowRoot?adoptedStyleSheets <- [|
                                                     mainLayoutCSS
                                                     mainTextCSS
                                                     layoutCSS
                                                     textCSS
                                                     appearanceCSS
                                                 |]
                                                 |> Array.map createSheet
            | _ -> ()),
        [| shadowRoot |> box<_> |]
    )

    Interop.createElement
        "signin-page"
        [
            attachShadowRoot
            prop.custom ("class", "page")

            prop.children []
        ]
