module MyPlanner.Client.View.Pages.Signin

open MyPlanner.Client
open Pages.Signin
open MyPlanner.Shared.Domain
open Feliz
open Fable.Core.JsInterop
open MyPlanner.Client.View.Components
open Browser.Dom
open MyPlanner.Client.View
open System
open Browser.Types
open Fable

ModalWindow.ensureDefined ()

let html : string =
    importDefault ("!!raw-loader!./_Pages/Tasks.html")


[<ReactComponent>]
let SigninView dispatch (model: Model) =

    let attachShadowRoot, shadowRoot = Util.useShadowRoot (html)

    Interop.createElement
        "signin-page"
        [
            attachShadowRoot
            prop.children [

            ]
        ]
