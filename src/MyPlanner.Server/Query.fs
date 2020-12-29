module MyPlanner.Server.Query

open MyPlanner.Shared.Domain

[<Interface>]
type ITaskQuery =
    abstract GetTasks: unit -> Task list Async
