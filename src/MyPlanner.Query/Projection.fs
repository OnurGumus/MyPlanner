module MyPlanner.Query.Projection

open MyPlanner.Command
open Akka.Persistence.Query
open Akka.Persistence.Query.Sql
open Common
open Serilog
open MyPlanner
open MyPlanner.Shared.Domain
open FSharp.Data.Sql
open FSharp.Data.Sql.Common
open Microsoft.Extensions.Configuration
open MyPlanner.Shared
open Akkling.Streams
open Akka.Streams
open Akka.Streams.Dsl

[<Literal>]
#if _VS
let arc = @"x86/"
#else
let arc = @"x64/"
#endif

[<Literal>]
let resolutionPath =
    __SOURCE_DIRECTORY__ + @"/../sqlite_" + arc

[<Literal>]
let connectionString =
    @"Data Source="
    + __SOURCE_DIRECTORY__
    + @"/../MyPlanner.Server/MyPlanner.db;"

type Sql =
    SqlDataProvider<Common.DatabaseProviderTypes.SQLITE, SQLiteLibrary=Common.SQLiteLibrary.MicrosoftDataSqlite, ConnectionString=connectionString, ResolutionPath=resolutionPath, CaseSensitivityChange=Common.CaseSensitivityChange.ORIGINAL>



let createTables (config: IConfiguration) =
    let ctx =
        Sql.GetDataContext(
            config
                .GetSection(
                    Constants.ConnectionString
                )
                .Value
        )

    let conn = ctx.CreateConnection()
    conn.Open()

    try
        let cmd = conn.CreateCommand()

        let offsets = "CREATE TABLE Offsets (
            OffsetName	TEXT,
            OffsetCount	INTEGER NOT NULL DEFAULT 0,
            PRIMARY KEY(OffsetName)
        );"
        cmd.CommandText <- offsets
        cmd.ExecuteNonQuery() |> ignore

        let tasks = "CREATE TABLE Tasks (
                Id TEXT NOT NULL, Version INTEGER NOT NULL, Title TEXT NOT NULL, Description TEXT NOT NULL,
                CONSTRAINT Tasks_PK PRIMARY KEY (Id)
            )"
        cmd.CommandText <- tasks
        cmd.ExecuteNonQuery() |> ignore

        let offset =
            ctx.Main.Offsets.``Create(OffsetCount)`` 0L

        offset.OffsetName <- "Tasks"
        ctx.SubmitUpdates()
        conn
    with ex ->
        printf "%A" ex
        conn

// type TaskEvent = TaskCreated of Task
// type DataEvent = TaskEvent of TaskEvent


QueryEvents.SqlQueryEvent
|> Event.add (fun sql -> Log.Debug("Executing SQL: {SQL}", sql))

let handleEvent (connectionString: string) (subQueue: ISourceQueue<_>) (envelop: EventEnvelope) =
    let ctx = Sql.GetDataContext(connectionString)
    Log.Information("Handle event {@Envelope}", envelop)

    // let dataEvent =
    //     match envelop.Event with
    //     | :? Message<Command.Domain.Task.Command, Command.Domain.Task.Event> as taskEvent ->
    //         match taskEvent with
    //         | Event ({
    //                      Event = Command.Domain.Task.TaskCreated task
    //                  }) ->
    //             let (Version v) = task.Version
    //             let (TaskId (ShortString tid)) = task.Id
    //             let (TaskTitle (ShortString title)) = task.Title
    //             let (TaskDescription (LongString desc)) = task.Description
    //             let row = ctx.Main.Tasks.Create(desc, title, v)
    //             row.Id <- tid
    //             Some (TaskEvent (TaskCreated task))
    //         | _ -> None
    //     | _ -> None

    // ctx.Main.Offsets.Individuals.Tasks.OffsetCount <- (envelop.Offset :?> Sequence).Value
    // ctx.SubmitUpdates()

    // match dataEvent with
    // | Some dataEvent -> subQueue.OfferAsync(dataEvent).Wait()
    // | _ -> ()





open Akkling.Streams
open MyPlanner.Command.Actor

let readJournal system =
    printfn "sql id:%A" SqlReadJournal.Identifier

    PersistenceQuery
        .Get(system)
        .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)

let init (connectionString: string) (actorApi: IActor) =
    let ctx = Sql.GetDataContext(connectionString)

    let offsetCount =
        ctx.Main.Offsets.Individuals.Tasks.OffsetCount

    let source =
        (readJournal actorApi.System)
            .EventsByTag("default", Offset.Sequence(offsetCount))

    System.Threading.Thread.Sleep(100)

    let subQueue =
        Source.queue OverflowStrategy.Backpressure 256

    let subSink = (Sink.broadcastHub 256)

    let runnableGraph =
        subQueue |> Source.toMat subSink Keep.both

    let queue, subRunnable =
        runnableGraph |> Graph.run (actorApi.Materializer)

    source
    |> Source.runForEach actorApi.Materializer (handleEvent connectionString queue)
    |> Async.StartAsTask
    |> ignore

    System.Threading.Thread.Sleep(1000)

    subRunnable
