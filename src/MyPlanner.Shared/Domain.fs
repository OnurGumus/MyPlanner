module MyPlanner.Shared.Domain

type Version = Version of int

let version0 = Version 0

type TaskId = TaskId of string

type Task = { Id: TaskId ; Version: Version }

module Command = 
    type  CreateTask = Task -> Result<Task,string> Async
