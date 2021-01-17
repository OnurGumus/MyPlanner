module MyPlanner.Test.Environments

open MyPlanner.Server.Query
open MyPlanner.Shared.Domain

type AppEnv(tasks: Task list) = 
    interface IQuery with
        member _.Query(?filter, ?orderby, ?thenby, ?take, ?skip) = 
            async { return tasks |> unbox }

