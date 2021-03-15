module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client
open Pages.Tasks
open MyPlanner.Shared.Domain
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Dom
open MyPlanner.Client.View

ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")

[<ReactComponent>]
let View dispatch (model: Model) =
    match model.Tasks with
    | [] -> Html.none
    | _ ->

        let attachShadowRoot, shadowRoot = Util.useShadowRoot(html)

        React.useEffect (
            (fun () ->
                match shadowRoot with
                | Some shadowRoot ->
                    shadowRoot.addEventListener ("TaskFromSubmitted", (fun e -> console.log e))
                | _ -> ()),
            [| shadowRoot |> box |]
        )

        Interop.createElement
            "task-list"
            [
                attachShadowRoot
                prop.children [
                    Html.div [
                        prop.slot "task-list"
                        prop.children [
                            Html.ol [
                                for t in model.Tasks do
                                    Html.li [ prop.textf "%A" t ]
                            ]
                        ]
                    ]
                ]
            ]
