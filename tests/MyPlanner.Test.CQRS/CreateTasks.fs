module MyPlanner.Test.CQRS.CreateTasks

open TickSpec
open MyPlanner.Command.API
open MyPlanner.Shared.Domain
open System.Threading
open MyPlanner.Query
open System.IO

[<Given>]
let ``there are no tasks in the system`` () = 
    let api = MyPlanner.Command.API.api obj
    Projection.init (api.ActorApi)
    api


[<When>]
let ``I create a task`` (api:IAPI) = 
    api.CreateTask {Id = TaskId "a"; Version =version0}
    |> Async.RunSynchronously


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` () = ()

[<Then>]
let ``I should see 1 task\(s\) listed`` (api:IAPI) = 
   Thread.Sleep 3000
   api.ActorApi.Stop().Wait()