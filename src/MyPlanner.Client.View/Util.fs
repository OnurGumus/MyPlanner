module MyPlanner.Client.View.Util

open Feliz
open Fable.Core
open Fable.React.Helpers
open Fable.Core.JsInterop
open Browser.Types

let reactShadowRoot props child =
    ofImport "default" "react-shadow-root" props child

importAll "construct-style-sheets-polyfill"

let dialogPolyfill : obj = import "default" "dialog-polyfill"

[<Global>]
type CSSStyleSheet() =
    class
    end

let sheet = new CSSStyleSheet()
sheet?replaceSync ("h1 { color: red; }")
Browser.Dom.document?adoptedStyleSheets <- [| sheet |]

let useShadowRoot (html: string) =
    let (shadowRoot: HTMLElement option), setRootTag = React.useState (None)

    let attachShadowRoot =
        prop.ref
            (fun x ->
                if x <> null && shadowRoot.IsNone then
                    setRootTag (Some(x?attachShadow {| mode = "open" |})))

    React.useEffect (
        (fun () ->
            shadowRoot
            |> Option.iter (fun s -> s.innerHTML <- html)),
        [| shadowRoot |> box |]
    )

    attachShadowRoot, shadowRoot
