module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client
open Pages.Tasks
open MyPlanner.Shared.Domain
open Feliz
open View.Util
open Fable.Core.JsInterop

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")

[<ReactComponent>]
let view dispatch (model: Model) =
    let list =
        Html.div [
            prop.slot "task-list"
            prop.children [
                Html.button [
                    prop.text "Create"
                    prop.onClick
                        (fun _ ->
                            dispatch (
                                TaskCreationRequested
                                    {
                                        Id = TaskId "onur3"
                                        Version = version0
                                    }
                            ))
                ]
                Html.ol [
                    for t in model.Tasks do
                        Html.ul [ prop.textf "%A" t ]
                ]
            ]
        ]

    loadHtmlInShadow html "" "task-list" [ list ]
