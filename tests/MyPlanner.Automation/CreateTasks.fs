module MyPlanner.Automation.CreateTasks


open canopy.configuration
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open OpenQA.Selenium


[<Given>]
let ``there are no tasks in the system`` () = 
    let tasks =
        [ ]
   // AppEnv(Host.config,[])
    Host.startHost (AppEnv(Host.config,[]))
    Host.startBrowser()
    url "http://localhost:8085"
    sleep 1

[<When>]
let ``I create a task`` () = 
    sleep 2
    let  shadowRoot:IWebElement = (js """return (document.querySelector("task-list")).shadowRoot""") |> unbox<_>
    let button = elementWithin "#create-button" shadowRoot 
    button |> click
    (elementWithin "[name='title']" shadowRoot) << "task title"
    (elementWithin "[name='description']" shadowRoot) << "task desc"
    (elementWithin "form button" shadowRoot) |> click

[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` () = sleep 1

[<Then>]
let ``I should see 1 task\(s\) listed`` () = 
    (elements "li").Length === 1
