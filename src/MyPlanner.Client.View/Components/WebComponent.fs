module MyPlanner.Client.View.Components.WebComponent

    open Fable.Core
    open Browser.Types
    open Fable.Core.JsInterop
    open Browser
    //We disable linter because F# coding convention is not good with html/js
    //fsharplint:disable

    //Below types are missing from Fable so we add it
    [<AllowNullLiteral>]
    type HTMLTemplateElement =
        inherit HTMLElement
        abstract content: DocumentFragment with get, set

    [<AllowNullLiteral>]
    type HTMLTemplateElementType =
        //cool way to generate the constctuctor
        [<EmitConstructor>]
        abstract Create: unit -> HTMLTemplateElement

    //We create our own ShadowRoot to interact with browser api.
    [<Global>]
    type ShadowRoot() =
        member this.appendChild(el: Browser.Types.Node) = jsNative
        member this.querySelector(selector: string): Browser.Types.HTMLElement = jsNative

    // The built in html element is missing below props so we use our own
    [<Global; AbstractClass>]
    [<AllowNullLiteral>]
    type HTMLElement() =
        member _.getAttribute(attr: string): obj = jsNative
        member _.setAttribute(attr: string, v: obj) = jsNative
        member _.attachShadow(obj): ShadowRoot = jsNative
        member _.dispatchEvent(e: CustomEvent): unit = jsNative
        abstract connectedCallback: unit -> unit
        default _.connectedCallback() = ()
        abstract attributeChangedCallback: string * obj * obj -> unit
        default _.attributeChangedCallback(_, _, _) = ()

    [<Global; AbstractClass>]
    [<AllowNullLiteral>]
    type HTMLButtonElement() = class
        inherit HTMLElement()
    end

    (* in your app add react-shadow-dom-retarget-events via yarn uncomment below code to make sure react components work fine
       then call it from your component with:  retargetEvents shadowRoot *)
    let retargetEvents: (ShadowRoot -> unit) =
        importDefault "react-shadow-dom-retarget-events"

    //special member for html web components
    [<Literal>]
    let observedAttributes = "observedAttributes"