namespace MyPlanner.Query

open MyPlanner.Shared.Domain

type GetTasks = unit -> Task list Async

[<Interface>]
type ITaskQuery =
    abstract GetTasks: GetTasks
