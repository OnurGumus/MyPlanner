module MyPlanner.Shared.Domain

type Version = Version of int64

let version0 = Version 0L

type ShortString = ShortString of string

type LongString = LongString of string

type TaskId = TaskId of ShortString

type TaskDescription = TaskDescription of LongString

type TaskTitle = TaskTitle of ShortString
type Task =
    {
        Id: TaskId
        Version: Version
        Title: TaskTitle
        Description: TaskDescription
    }

module Command =
    type CreateTask = Task -> Result<Task, string> Async
