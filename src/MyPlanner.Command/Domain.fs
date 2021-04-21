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

module User =
    type Command = Register of User

    type Event = VerificationRequested of User * VerificationCode

    let random = Random()
    type State = { User : User; Verification : VerificationCode option;}

    let actorProp toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<_>) =

        let mediatorS = retype mediator
        let sendToSagaStarter =
            SagaStarter.toSendMessage mediatorS mailbox.Self

        let rec set (state: State option * int64) =
            actor {
                let! msg = mailbox.Receive()
                Log.Debug("Message {@MSG}", box msg)

                match msg, state with
                | Recovering mailbox (Event { Event = VerificationRequested(user, vCode); Version = version }), _ ->
                    return! ({ User = user; Verification = Some vCode} |> Some, version) |> set

                | Command { Command = (Register user); CorrelationId = ci }, (None, v) ->
                    let v = v + 1L
                    let e =
                        ({ user with Version = Version v}, (VerificationCode 334)) |> VerificationRequested

                    let event = toEvent ci (e) v
                    sendToSagaStarter event ci
                    return! event |> Event |> Persist

                | Persisted mailbox (Event ({ Event = VerificationRequested(user, vCode); Version = v } as e)), _ ->
                    Log.Information "persisted"
                    SagaStarter.publishEvent mailbox mediator e
                    return! (Some {User = user; Verification = Some vCode}, v) |> set

                | _ -> return Unhandled
            }

        set (None, 0L)

    let init toEvent (actorApi: IActor) =
        AkklingHelpers.entityFactoryFor actorApi.System shardResolver (nameof (User))
        <| propsPersist (actorProp toEvent (typed actorApi.Mediator))
        <| false

    let factory toEvent actorApi entityId =
        (init toEvent actorApi).RefFor DEFAULT_SHARD entityId


module UserVerificationSaga =
    type State =
        | NotStarted
        | WaitingForVerification
    type Event =
        | StateChanged of State
        interface IDefaultTag

    let actorProp toEvent (mediator: IActorRef<_>) (mailbox: Eventsourced<obj>) =
        let rec set (state: State) =
            let cid =
                (mailbox.Self.Path.Name |> SagaStarter.toRawGuid)
            actor {
                let! msg = mailbox.Receive()
                match msg, state with
                | SagaStarter.SubscrptionAcknowledged mailbox _, _ ->
                    // notify saga starter about the subscription completed
                    match state with
                    | NotStarted -> return! WaitingForVerification |> StateChanged |> box |> Persist
                    | WaitingForVerification -> return! set state
                | PersistentLifecycleEvent ReplaySucceed, _ ->
                    SagaStarter.subscriber mediator mailbox
                    return! set state
                | Recovering mailbox (:? Event as e), _ ->
                    //replay the recovery
                    match e with
                    | StateChanged s -> return! set s
                | Persisted mailbox (:? Event as e), _ ->
                    match e with
                    | StateChanged WaitingForVerification ->
                        SagaStarter.cont mediator
                        return! set state
                    | _ -> return! set state
                | _ -> return! set state
            }
        set NotStarted
    let init toEvent (actorApi: IActor)=
        (AkklingHelpers.entityFactoryFor actorApi.System shardResolver "UserVerificationSaga"
             <| propsPersist (actorProp toEvent (typed actorApi.Mediator))
             <| true)
    let factory toEvent actorApi entityId =
        (init toEvent actorApi).RefFor DEFAULT_SHARD entityId

let sagaCheck toEvent actorApi (o: obj) =
    match o with
    | :? (Event<User.Event>) as e ->
        match e with
        | { Event = User.VerificationRequested _ } -> [ (UserVerificationSaga.factory toEvent actorApi, "userverification") ]
    | _ -> []

open Akkling.Cluster.Sharding

[<Interface>]
type IDomain =
    abstract ActorApi: IActor
    abstract Clock: IClock
    abstract UserFactory: string -> IEntityRef<Message<User.Command, User.Event>>


let api (clock: IClock) (actorApi: IActor) =

    let toEvent ci = Common.toEvent clock ci
    SagaStarter.init actorApi.System actorApi.Mediator (sagaCheck toEvent actorApi)

    User.init
    |> sprintf "User initialized: %A"
    |> Log.Debug

    UserVerificationSaga.init
    |> sprintf "OrderSaga initialized %A"
    |> Log.Debug

    System.Threading.Thread.Sleep(1000)

    { new IDomain with
        member _.Clock = clock
        member _.ActorApi = actorApi
        member _.UserFactory entityId = User.factory toEvent actorApi entityId }
