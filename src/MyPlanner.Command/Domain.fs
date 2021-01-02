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
open Actor


[<Literal>]
let DEFAULT_SHARD = "default-shard"

let shardResolver = fun _ -> DEFAULT_SHARD

module Task =
    type Command = CreateTask of Task

    type Event = TaskCreated of Task

    let actorProp toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<_>) =

        let rec set (state: Task option * int64) =
            actor {
                let! msg = mailbox.Receive()
                Log.Debug("Message {@MSG}", box msg)

                match msg, state with

                | Command { Command = (CreateTask t)
                            CorrelationId = ci },
                  (None, v) ->
                    let task =
                        { t with Version = Version(v + 1L) } |> TaskCreated

                    let event = toEvent ci (task) (v + 1L)

                    return! event |> Event |> Persist

                | Persisted mailbox (Event ({ Event = TaskCreated t; Version = v } as e)), _ ->
                    Log.Information "persisted"
                    SagaStarter.publishEvent mailbox mediator e
                    return! (Some t, v) |> set

                | _ -> return Unhandled
            }

        set (None, 0L)

    let init toEvent (actorApi: IActor) =
        AkklingHelpers.entityFactoryFor actorApi.System shardResolver (nameof (Task))
        <| propsPersist (actorProp toEvent (typed actorApi.Mediator))
        <| false

    let factory toEvent actorApi entityId =
        (init toEvent actorApi).RefFor DEFAULT_SHARD entityId

let sagaCheck (o: obj) = []

open Akkling.Cluster.Sharding

[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract TaskFactory: string -> IEntityRef<Message<Task.Command, Task.Event>>


let api (clock: IClock) (actorApi: IActor) =

    let toEvent ci = Common.toEvent clock ci
    SagaStarter.init actorApi.System actorApi.Mediator sagaCheck

    Task.init
    |> sprintf "Task initialized: %A"
    |> Log.Debug

    System.Threading.Thread.Sleep(1000)

    { new IDomain with
        member _.Clock = clock
        member _.ActorApi = actorApi
        member _.TaskFactory entityId = Task.factory toEvent actorApi entityId }
