module MyPlanner.Client.View.Main

open MyPlanner.Client.Main

open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Util

let mainLayoutCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/layout.css")

let mainTextCSS : string =
    importDefault ("!!raw-loader!./_wwwroot/text.css")


Browser.Dom.document?adoptedStyleSheets <- [| mainLayoutCSS; mainTextCSS |] |> Array.map createSheet

let view (model: Model) dispatch =
    match model.Page with
    | Some (Page.Signin (smodel)) -> Pages.Signin.SigninView(SigninMsg >> dispatch) smodel

    | _ -> Html.none
