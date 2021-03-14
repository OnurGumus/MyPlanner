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

    let shadowRoot : HTMLElement Lazy =
        lazy (!!document.querySelector("task-list")?shadowRoot)

    React.useLayoutEffect
        (fun () ->


            let form : HTMLFormElement =
                !!shadowRoot.Value.querySelector ("form")

            let onSubmit (e: Event) =
                e.preventDefault ()
                shadowRoot.Value.querySelector("#create-task-dialog")?isVisible <- false

            if form <> null then
                form.onsubmit <- onSubmit)

    let list =
        Html.div [
            prop.slot "task-list"
            prop.children [

                Html.button [
                    prop.text "Create"
                    prop.onClick (fun _ -> shadowRoot.Value.querySelector("#create-task-dialog")?isVisible <- true)
                ]
                Html.ol [
                    for t in model.Tasks do
                        Html.li [ prop.textf "%A" t ]
                ]


            ]


        ]

    loadHtmlInShadow html "" "task-list" [ list ]
