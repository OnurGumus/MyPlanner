module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client
open Pages.Tasks
open MyPlanner.Shared.Domain
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Types
open Browser.Dom


ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")


[<ReactComponent>]
let View dispatch (model: Model) =
    match model.Tasks with
    | [] -> Html.none
    | _ ->

        let (shadowRoot: HTMLElement option), setRootTag = React.useState (None)

        let attachShadowRoot =
            prop.ref
                (fun x ->
                    if x <> null && shadowRoot.IsNone then
                        setRootTag (Some(x?attachShadow {| mode = "open" |})))

        React.useEffect (
            (fun () ->
                match shadowRoot with
                | Some shadowRoot ->
                    shadowRoot.innerHTML <- html
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
