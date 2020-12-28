module MyPlanner.Server.Query

open MyPlanner.Shared.Model

[<Interface>]
type ITaskQuery =
    abstract GetTasks: unit -> Task list Async
