namespace MyPlanner.Query

open MyPlanner.Shared.Model

type GetTasks = unit -> Task list Async

[<Interface>]
type ITaskQuery =
    abstract GetTasks: GetTasks
