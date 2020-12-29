module MyPlanner.Server.Command

open MyPlanner.Shared.Domain.Command

[<Interface>]
type ITaskCommand =
    abstract CreateTask: CreateTask
