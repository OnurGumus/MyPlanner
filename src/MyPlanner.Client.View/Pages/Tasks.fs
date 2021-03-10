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
            prop.ref(fun x -> if x <> null then printf "registering dialog" ; dialogPolyfill?registerDialog (x.childNodes.[0]))
            prop.slot "task-list"
            prop.children [

                Html.dialog[
                    prop.ref(fun x -> if x <> null then printf "%A"("registering dialog2"); )
                    match model.DialogStatus with
                     | Open -> prop.ref(fun x -> if x <> null then x.setAttribute ("open",""))
                     | _ -> prop.ref(fun x -> if x <> null then x.removeAttribute("open"))
                    prop.children[
                        Html.p "Create a task"
                        Html.button [ prop.text "Submit"; prop.onClick(fun _ -> dispatch DialogClosed)]
                    ]
                ]
                Html.button [
                    prop.text "Create"
                    prop.onClick
                        (fun _ ->
                            dispatch ( DialogOpened
                                // TaskCreationRequested
                                //     {
                                //         Id = TaskId "onur3"
                                //         Version = version0
                                //     }
                            ))
                ]
                Html.ol [
                    for t in model.Tasks do
                        Html.li [ prop.textf "%A" t ]
                ]


            ]
        ]

    loadHtmlInShadow html "" "task-list" [ list ]
