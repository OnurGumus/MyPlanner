module MyPlanner.Command.Domain


open Akkling
open Akkling.Persistence
open AkklingHelpers
open Akka
open Common
open Akka.Cluster.Sharding
open Serilog
open System
open Akka.Cluster.Tools.PublishSubscribe
open NodaTime
open MyPlanner.Shared.Domain

let clockInstance: IClock = upcast SystemClock.Instance

[<Literal>]
let DEFAULT_SHARD = "default-shard"

let shardResolver = fun _ -> DEFAULT_SHARD

let toEvent ci = Common.toEvent clockInstance ci

module Task =
    type Command = CreateTask of Task

    type Event = TaskCreated of Task

    let actorProp (mediator: IActorRef<Publish>) (mailbox: Eventsourced<_>) =

        let rec set (state: Task option * int) =
            actor {
                let! msg = mailbox.Receive()
                Log.Debug("Message {@MSG}", box msg)

                match msg, state with

                | Command { Command = (CreateTask t)
                            CorrelationId = ci },
                  (None, v) -> return! set state

                | Persisted mailbox (Event ({ Event = TaskCreated t; Version = v } as e)), _ ->
                    Log.Information "persisted"
                    SagaStarter.publishEvent mailbox mediator e
                    return! (Some t, v) |> set

                | _ -> return Unhandled
            }

        set (None, 0)

    let init =
        AkklingHelpers.entityFactoryFor Actor.system shardResolver (nameof (Task))
        <| propsPersist (actorProp (typed Actor.mediator))
        <| false

    let factory entityId = init.RefFor DEFAULT_SHARD entityId

let sagaCheck (o: obj) = []

let init () =
    SagaStarter.init Actor.system Actor.mediator sagaCheck

    Task.init
    |> sprintf "Task initialized: %A"
    |> Log.Debug

    System.Threading.Thread.Sleep(1000)
