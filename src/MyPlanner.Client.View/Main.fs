module MyPlanner.Client.View.Main

open MyPlanner.Client.Main

open Feliz

let view (model: Model) dispatch =
    match model.Page with
    | Some (Page.Signin (smodel)) -> Pages.Signin.SigninView(SigninMsg >> dispatch) smodel

    | _ -> Html.none
