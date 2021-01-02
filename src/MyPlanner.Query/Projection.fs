module MyPlanner.Query.Projection

open MyPlanner.Command
open Akka.Persistence.Query
open Akka.Persistence.Query.Sql
open Common
open Serilog
open Domain


let handleEvent (envelop: EventEnvelope) =
    Log.Information("Handle event {@Envelope}", envelop)

    try
        match envelop.Event with
        | :? Message<Task.Command, Task.Event> as task -> Serilog.Log.Information(sprintf "Task is %A" task)
        | _ -> ()

    with e -> printf "%A" e



open Akkling.Streams
open MyPlanner.Command.Actor


let readJournal system =
    printfn "sql id:%A" SqlReadJournal.Identifier

    PersistenceQuery
        .Get(system)
        .ReadJournalFor<SqlReadJournal>(SqlReadJournal.Identifier)


let init (actorApi: IActor) =
    let source =
        (readJournal actorApi.System)
            .EventsByTag("default", Offset.Sequence(0L))

    System.Threading.Thread.Sleep(100)

    source
    |> Source.runForEach actorApi.Materializer handleEvent
    |> Async.StartAsTask
    |> ignore

    System.Threading.Thread.Sleep(1000)
