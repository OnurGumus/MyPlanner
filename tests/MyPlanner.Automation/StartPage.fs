module MyPlanner.Automation.StartPage
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open MyPlanner.Shared.Domain
open OpenQA.Selenium

let getPageShadowRoot page = 
    let  shadowRoot:IWebElement = (js $"""return (document.querySelector("${page}")).shadowRoot""") |> unbox<_>
    shadowRoot
[<Given>]
let ``I am not logged in`` () = 
    AppEnv(Host.config)


[<When>]
let ``I visit the start page`` ((appEnv: AppEnv)) = 
    Host.startHost (appEnv)
    Host.startBrowser()
    url "http://localhost:8085"

[<Then>]
let ``I should be at the signin page`` () = 
    waitFor(fun () -> (currentUrl().Contains "signin"))

[<When>]
let ``I click to signup link`` () = 
    let shadowRoot = getPageShadowRoot "sigin-page"
    let button = elementWithin "#signup" shadowRoot 
    button |> click

[<Then>]
let ``I should be at the signup page`` () = 
    waitFor(fun () -> (currentUrl().Contains "signin"))