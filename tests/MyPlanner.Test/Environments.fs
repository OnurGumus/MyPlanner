module MyPlanner.Test.Environments

open MyPlanner.Server.Query
open MyPlanner.Shared.Domain
open MyPlanner.Server.Command
open Microsoft.Extensions.Configuration

type AppEnv(config: IConfiguration, tasks: Task list) =
    let mutable tasks = tasks
    let mutable dataCallback = Unchecked.defaultof<_>

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

    interface IQuery with
        member _.Query(?filter, ?orderby, ?thenby, ?take, ?skip) = async { return tasks |> unbox }

        member _.Subscribe(cb) =
            dataCallback <- cb
            Unchecked.defaultof<_>

    interface ITaskCommand with
        member _.CreateTask =
            fun t ->
                tasks <- t :: tasks
                dataCallback (MyPlanner.Query.Projection.TaskEvent(MyPlanner.Query.Projection.TaskCreated t))
                async { return Ok t }
