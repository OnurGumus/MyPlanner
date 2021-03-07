module MyPlanner.Client.View.Main
open MyPlanner.Client.Main

open Feliz

let view (model:Model) dispatch = 
    match model.Page with
    | Some (Tasks (Some tasksModel )) ->
        Pages.Tasks.view (TasksMsg >> dispatch) tasksModel 

    | _ -> Html.none