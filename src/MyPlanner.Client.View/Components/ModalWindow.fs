module MyPlanner.Client.View.Components.ModalWindow

open Fable.Core
open Browser
open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open Fable.Core.JS
open Fable.Core.DynamicExtensions
open MyPlanner.Client.View.Components.WebComponent

//fsharplint:disable
let template : HTMLTemplateElement = !! document.createElement "template"


let private html : string =
    importDefault "!!raw-loader!./_Components/ModalWindow.html"

template.innerHTML <- html

//Define some constants
[<Literal>]
let TAG = "modal-window"

[<Literal>]
let IS_ACTIVE = "is-active"

[<Literal>]
let VISIBLE = "visible"

// we are writing our component below
[<AllowNullLiteral;AttachMembers>]
type ModalWindow() as this =
    inherit HTMLElement()

    let el : Browser.Types.HTMLElement = !!this
    let shadowRootRef : ShadowRoot = base.attachShadow {| mode = "open" |}
    do
        //clone the html text and append to the child
        let clone = template.content.cloneNode true
        shadowRootRef.appendChild clone

    static member observedAttributes = [| VISIBLE |]

    member _.isVisible
        with get () = el.hasAttribute VISIBLE
        and set value =
            if value then
                el.setAttribute (VISIBLE, "")
            else
                el.removeAttribute VISIBLE

    member private this.render() =
        let wrapper = shadowRootRef.querySelector (".wrapper")

        if this.isVisible then
            wrapper.classList.add "visible"
        else
            wrapper.classList.remove "visible"

    override this.attributeChangedCallback(name, oldVal, newVal) = this.render ()

if not (window?customElements?get TAG) then
    window?customElements?define (TAG, jsConstructor<ModalWindow>)


[<AllowNullLiteral;AttachMembers>]
type FancyButton() as this =
    inherit HTMLButtonElement()
    let el : Browser.Types.HTMLElement = !!this
    do
        el.addEventListener("click", fun e -> el.innerHTML <- "I was clicked");


if not (window?customElements?get "fancy-button") then
    window?customElements?define ("fancy-button", jsConstructor<FancyButton>,{|extends = "button"|})



// dummy function to ensure the above code is run
let ensureDefined () = ()
