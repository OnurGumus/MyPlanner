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
[<AllowNullLiteral>]
type ModalWindow() as this =
    inherit HTMLElement()

    let el : Browser.Types.HTMLElement = !!this
    let shadowRootRef : ShadowRoot = base.attachShadow {| mode = "open" |}
    // see the html above. We use lazy because the dom element isn't available at this point.
    let attachEventHandlers () =
        let cancelButton = shadowRootRef.querySelector (".cancel")

        cancelButton.addEventListener (
            "click",
            fun _ ->
                el.dispatchEvent (CustomEvent.Create("cancel"))
                |> ignore

                el.removeAttribute ("visible")
        )

        let okButton = shadowRootRef.querySelector (".ok")

        okButton.addEventListener (
            "click",
            fun _ ->
                el.dispatchEvent (CustomEvent.Create("ok"))
                |> ignore

                el.removeAttribute ("visible")
        )

    do
        //clone the html text and append to the child
        let clone = template.content.cloneNode true
        shadowRootRef.appendChild clone
        attachEventHandlers ()


    abstract isVisible : bool with get, set

    override _.isVisible
        with get () = el.hasAttribute VISIBLE
        and set value =
            if value then
                el.setAttribute (VISIBLE, "")
            else
                el.removeAttribute VISIBLE

    member this.render() =
        let wrapper = shadowRootRef.querySelector (".wrapper")

        if this.isVisible then
            wrapper.classList.add "visible"
        else
            wrapper.classList.remove "visible"

    override this.attributeChangedCallback(name, oldVal, newVal) = this.render ()


attachStaticGetter<ModalWindow, _> observedAttributes (fun () -> [| VISIBLE |])

if not (window?customElements?get TAG) then
    window?customElements?define (TAG, jsConstructor<ModalWindow>)
// dummy function to ensure the above code is run
let ensureDefined () = ()
