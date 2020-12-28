module MyPlanner.Test.Environments
open MyPlanner.Server.Query
open MyPlanner.Shared.Model

type AppEnv() =
    interface ITaskQuery with
        member _.GetTasks() = async { return [Task.NA] } 