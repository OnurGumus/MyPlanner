module MyPlanner.Test.Environments

open MyPlanner.Server.Query
open MyPlanner.Shared.Domain
open MyPlanner.Server.Command

type AppEnv(tasks: Task list) =
    let mutable tasks = tasks
    interface IQuery with
        member _.Query(?filter, ?orderby, ?thenby, ?take, ?skip) = async { return tasks |> unbox }
    interface ITaskCommand with
        member _.CreateTask = fun t ->
             tasks <- t::tasks
             async{ return Ok t}
