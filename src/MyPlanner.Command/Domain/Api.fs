module MyPlanner.Command.Domain.Api

open MyPlanner.Command
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
open Akkling.Cluster.Sharding
open User


let sagaCheck toEvent actorApi (clock:IClock) (o: obj) =
    match o with
    | :? (Event<User.Event>) as e ->
        match e with
        | { Event = User.RegistrationRequested _ } ->
            [ (UserRegistrationSaga.factory toEvent actorApi clock, "userregistration") ]
        | _ -> []
    | _ -> []



[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract UserFactory: string -> IEntityRef<Message<User.Command, User.Event>>


let api (clock: IClock) (actorApi: IActor) =

    let toEvent ci = Common.toEvent clock ci
    SagaStarter.init actorApi.System actorApi.Mediator (sagaCheck toEvent actorApi clock)

    User.init toEvent actorApi
    |> sprintf "User initialized: %A"
    |> Log.Debug

    UserRegistrationSaga.init toEvent actorApi clock
    |> sprintf "OrderSaga initialized %A"
    |> Log.Debug

    EmailService.init actorApi.System actorApi.Mediator
    System.Threading.Thread.Sleep(1000)

    { new IDomain with
        member _.Clock = clock
        member _.ActorApi = actorApi
        member _.UserFactory entityId = User.factory toEvent actorApi entityId }
