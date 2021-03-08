module MyPlanner.Client.View.Util

open Feliz
open Fable.React.Helpers

let reactShadowRoot child =
    ofImport "default" "react-shadow-root" [] child

let fragment props children =
    ofImport "default" "react-dom-fragment" props children

let inline loadHtmlInShadow html style tag (slots: ReactElement list) =
    Interop.reactElementWithChildren
        tag
        [
            reactShadowRoot [
                fragment
                    ({|
                         dangerouslySetInnerHTML = {| __html = (style + html) |}
                     |})
                    []
            ]
            slots |> ofList
        ]
