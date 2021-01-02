module MyPlanner.Command.Actor

open System.Collections.Immutable
open Akka.Streams
open Akka.Persistence.Journal
open Akka.Actor
open Akka.Cluster
open Akka.Cluster.Tools.PublishSubscribe
open Akka.Persistence.Sqlite
open Akkling

let private defaultTag = ImmutableHashSet.Create("default")

type private Tagger =
    interface IWriteEventAdapter with
        member _.Manifest _ = ""
        member _.ToJournal evt = box <| Tagged(evt, defaultTag)

    new() = {  }

[<Interface>]
type IActor =
    abstract Mediator: Akka.Actor.IActorRef
    abstract Materializer: ActorMaterializer
    abstract System: ActorSystem
    abstract SubscribeForCommand: Common.CommandHandler.Command<'a, 'b> -> Async<Common.Event<'b>>
    abstract Stop: unit -> System.Threading.Tasks.Task

let api config =
    let system = System.create "cluster-system" config

    SqlitePersistence.Get(system) |> ignore

    Cluster.Get(system).SelfAddress
    |> Cluster.Get(system).Join

    let mediator = DistributedPubSub.Get(system).Mediator

    let mat = ActorMaterializer.Create(system)

    let subscribeForCommand command =
        Common.CommandHandler.subscribeForCommand system (typed mediator) command
    { new IActor with
        member _.Mediator = mediator
        member _.Materializer = mat
        member _.System = system
        member _.SubscribeForCommand command = subscribeForCommand command
        member _.Stop() = system.Terminate() }
