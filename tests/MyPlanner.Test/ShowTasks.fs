module MyPlanner.Test.ShowTasks
open MyPlanner.Client
open TickSpec


[<Given>]
let ``N is a positive integer`` () = ()
  

[<Given>]
let ``there are N tasks in the system`` () = Environments.AppEnv()

[<When>]
let ``I visit url /tasks`` (appEnv : Environments.AppEnv) =       
  ElmishLoop.runWithDefaults appEnv  (Some(Main.Route.Tasks)) |> ignore
  System.Threading.Thread.Sleep 1000

[<Then>]
let ``I should see N tasks listed``() = ()