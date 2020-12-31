module MyPlanner.Test.CQRS.CreateTasks

open TickSpec
open MyPlanner.Command.API
open MyPlanner.Shared.Domain
open System.Threading
open MyPlanner.Query

[<Given>]
let ``there are no tasks in the system`` () = 
    Projection.init()


[<When>]
let ``I create a task`` () = 
    createTask {Id = TaskId "a"; Version =version0}
    |> Async.RunSynchronously


[<Then>]
let ``the task should be created successfully`` () = ()

[<When>]
let ``I visit url /tasks`` () = ()

[<Then>]
let ``I should see 1 task\(s\) listed`` () = 
   Thread.Sleep 3000
   MyPlanner.Command.Domain.stop().Wait()
