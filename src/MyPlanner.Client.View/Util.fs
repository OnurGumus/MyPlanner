module MyPlanner.Client.View.Util

open Feliz
open Fable.Core
open Fable.React.Helpers
open Fable.Core.JsInterop

let reactShadowRoot props child =
    ofImport "default" "react-shadow-root" props child

importAll "construct-style-sheets-polyfill"

let dialogPolyfill : obj = import  "default" "dialog-polyfill"

[<Global>]
type CSSStyleSheet() = class end

let sheet = new CSSStyleSheet();
sheet?replaceSync("h1 { color: red; }");
Browser.Dom.document?adoptedStyleSheets <- [|sheet|];

let inline loadHtmlInShadow html style tag (slots: ReactElement list) =
    Interop.reactElementWithChildren
        tag
        [
            reactShadowRoot {|stylesheets = [|sheet|]|} [

                Html.span [prop.dangerouslySetInnerHTML (style + html)]
            ]
            slots |> ofList
        ]
