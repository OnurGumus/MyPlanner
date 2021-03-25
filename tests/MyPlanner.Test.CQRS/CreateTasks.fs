module MyPlanner.Test.CQRS.CreateTasks

open TickSpec
open MyPlanner.Command.API
open MyPlanner.Shared.Domain
open System.Threading
open MyPlanner.Query
open System.IO
open Akkling
open Expecto
open Microsoft.Extensions.Configuration
open Hocon.Extensions.Configuration
open Akkling.Streams



[<Given>]
let ``there are no tasks in the system`` () =

    let configBuilder = ConfigurationBuilder()

    let config =
        configBuilder.AddHoconFile("config.hocon").Build()

    let conn =
        MyPlanner.Query.Projection.createTables (config)

    let commandApi =
        MyPlanner.Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        MyPlanner.Query.API.api config commandApi.ActorApi

    conn, commandApi, queryApi


[<When>]
let ``I create a task`` (api: IAPI, (qapi : MyPlanner.Query.API.IAPI)) =
    let tasks = ResizeArray()
   
    qapi.Subscribe(fun x-> tasks.Add x) |> ignore
    api.CreateTask
        { Id = "test_task" |> ShortString.ofString |> TaskId
          Version = version0
          Title = TaskTitle(ShortString.ofString "title")
          Description = TaskDescription(LongString.ofString "desc") }
    |> Async.RunSynchronously,
    tasks

[<Then>]
let ``the task should be created successfully`` (result: Result<Task, string>) =
    match result with
    | Ok _ -> ()
    | Error _ -> "task failed" |> Expect.isTrue false

[<When>]
let ``I visit url /tasks`` (queryApi: MyPlanner.Query.API.IAPI) =
    Thread.Sleep 3000
    queryApi.Query<Task>() |> Async.RunSynchronously

[<Then>]
let ``I should see 1 task\(s\) listed`` (tasks: Task list, commandApi: MyPlanner.Command.API.IAPI) =
    Expect.equal tasks.Length 1 "more than 1 tasks"
    commandApi.ActorApi.Stop().Wait()
    Expect.equal tasks.Length 1 "Stream has not returned the task"
