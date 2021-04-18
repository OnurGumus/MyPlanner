module MyPlanner.Automation.StartPage
open canopy.classic
open canopy.types
open TickSpec
open MyPlanner.Test.Environments
open MyPlanner.Shared.Domain

[<Given>]
let ``I am not logged in`` () = 
    AppEnv(Host.config)


[<When>]
let ``I visit the start page`` ((appEnv: AppEnv)) = 
    Host.startHost (appEnv)
    Host.startBrowser()
    url "http://localhost:8085"

[<Then>]
let ``I should be redirect to signin page`` () = 
    sleep 5
    currentUrl() |> contains "signin"
