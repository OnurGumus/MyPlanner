module MyPlanner.Client.View.Pages.Tasks

open MyPlanner.Client.Pages.Tasks
open Feliz
open MyPlanner.Shared.Domain

[<ReactComponent>]
let view dispatch (model: Model) =
    React.fragment[
        Html.button[ prop.text "Create" ; prop.onClick(fun _ -> dispatch (TaskCreationRequested {Id = TaskId "onur3"; Version = version0}))]
        Html.ol [
            for t in model.Tasks do
                Html.ul [ prop.textf "%A" t ]
        ]
    ]
