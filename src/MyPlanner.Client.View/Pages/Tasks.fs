module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client
open Pages.Tasks
open MyPlanner.Shared.Domain
open Feliz
open View.Util
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Dom
open Browser.Types


ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")


[<ReactComponent>]
let View dispatch (model: Model) =
    match model.Tasks with
    | [] -> Html.none
    | _ ->

        let shadowRoot, setRootTag = React.useState (None)
        let attachShadowRoot =
            prop.ref
                (fun x ->
                    if x <> null && shadowRoot.IsNone then
                        setRootTag (Some(x?attachShadow {|mode = "open"|})))

        React.useEffect
            (fun () ->
                match shadowRoot with
                | Some shadowRoot ->
                    shadowRoot?innerHTML <- html
                    let form : HTMLFormElement = !!shadowRoot?querySelector("form")

                    let onSubmit (e: Event) =
                        e.preventDefault ()
                        shadowRoot?querySelector("#create-task-dialog")?isVisible <- false

                    form.onsubmit <- onSubmit
                 | _ ->()
                )

        Interop.createElement "task-list" [
            attachShadowRoot
            prop.children [
                Html.div [
                    prop.slot "task-list"
                    prop.children [
                        Html.button [
                            prop.text "Create"
                            prop.onClick (fun _ -> shadowRoot.Value?querySelector("#create-task-dialog")?isVisible <- true)
                        ]
                        Html.ol [
                            for t in model.Tasks do
                                Html.li [ prop.textf "%A" t ]
                        ]
                    ]
                ]
             ]
        ]

