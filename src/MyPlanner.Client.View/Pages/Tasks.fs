module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client
open Pages.Tasks
open MyPlanner.Shared.Domain
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Dom
open MyPlanner.Client.View
open System
open Browser.Types

ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")

let createTaskFrom (formData: FormData) =
    {
        Id = TaskId(ShortString(Guid.NewGuid().ToString()))
        Version = version0
        Title = TaskTitle(ShortString(!! formData.get "title"))
        Description = TaskDescription(LongString(!! formData.get "title"))
    }

[<ReactComponent>]
let TasksView dispatch (model: Model) =

    let attachShadowRoot, shadowRoot = Util.useShadowRoot (html)

    React.useEffect (
        (fun () ->
            match shadowRoot with
            | Some shadowRoot ->
                shadowRoot.addEventListener (
                    "TaskFormSubmitted",
                    (fun e ->
                        e?detail
                        |> createTaskFrom
                        |> TaskCreationRequested
                        |> dispatch)
                )
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
                        Html.ul [
                            for t in model.Tasks do
                                Html.li [ prop.textf "%A-%A" t.Title t.Description ]
                        ]
                    ]
                ]
            ]
        ]
