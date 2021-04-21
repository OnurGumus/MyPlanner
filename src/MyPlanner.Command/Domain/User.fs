module MyPlanner.Command.Domain.User

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


[<Literal>]
let DEFAULT_SHARD = "default-shard"

let shardResolver = fun _ -> DEFAULT_SHARD

module User =
    type Command = Register of User | WaitForVerification | Verify of VerificationCode

    type Event =
            | RegistrationRequested of User * VerificationCode
            | VerificationEmailSent
            | VerificationFailed
            | VerificationSuccessful

    type Verification =  NotVerified of VerificationCode | Verified

    let random = Random()
    type State = { User : User; Verification : Verification;}

    let actorProp toEvent (mediator: IActorRef<Publish>) (mailbox: Eventsourced<_>) =

        let mediatorS = retype mediator
        let sendToSagaStarter =
            SagaStarter.toSendMessage mediatorS mailbox.Self

        let rec set (state: State option * int64) =
            actor {
                let! msg = mailbox.Receive()
                Log.Debug("Message {@MSG}", box msg)

                match msg, state with
                | Recovering mailbox (Event { Event = RegistrationRequested(user, vCode); Version = version }), _ ->
                    return! ({ User = user; Verification = NotVerified vCode} |> Some, version) |> set

                | Command { Command = (Register user); CorrelationId = ci }, (None, v) ->
                    let v = v + 1L
                    let e =
                        ({ user with Version = Version v}, (VerificationCode 334)) |> RegistrationRequested

                    let event = toEvent ci (e) v
                    sendToSagaStarter event ci
                    return! event |> Event |> Persist

                | Command { Command = (Verify userVCode); CorrelationId = ci }, (Some { Verification = NotVerified vCode }, v)->
                    let e =
                            if userVCode <> vCode then
                                VerificationFailed
                            else
                                VerificationSuccessful

                    let event = toEvent ci (e) v
                    sendToSagaStarter event ci
                    return! event |> Event |> Persist

                | Command { Command = (WaitForVerification); CorrelationId = ci }, (Some { Verification = NotVerified vCode }, v)->
                    let event = toEvent ci VerificationEmailSent v
                    SagaStarter.publishEvent mailbox mediator (event)
                    return! set state

                | Persisted mailbox (Event ({ Event = RegistrationRequested(user, vCode); Version = v } as e)), _ ->
                    Log.Information "persisted"
                    SagaStarter.publishEvent mailbox mediator e
                    return! (Some {User = user; Verification = NotVerified vCode}, v) |> set

                | Persisted mailbox (Event ({ Event = VerificationSuccessful} as e)), _
                | Persisted mailbox (Event ({ Event = VerificationFailed} as e)), _ ->
                    Log.Information "persisted"
                    SagaStarter.publishEvent mailbox mediator e
                    return! state |> set


                | _ -> return Unhandled
            }

        set (None, 0L)

    let init toEvent (actorApi: IActor) =
        AkklingHelpers.entityFactoryFor actorApi.System shardResolver (nameof (User))
        <| propsPersist (actorProp toEvent (typed actorApi.Mediator))
        <| false

    let factory toEvent actorApi entityId =
        (init toEvent actorApi).RefFor DEFAULT_SHARD entityId


module UserRegistrationSaga =
    type State =
        | NotStarted
        | WaitingForVerification
        | Completed
    type Event =
        | StateChanged of State
        interface IDefaultTag

    let actorProp toEvent (actorApi:IActor) (clockInstance:IClock) (mediator: IActorRef<_>) (mailbox: Eventsourced<obj>) =
        let cid =
            (mailbox.Self.Path.Name |> SagaStarter.toCid)

        let rec set (state: State) =


            let waitForVerification () =
                    ({
                         Command = User.WaitForVerification
                         CreationDate = clockInstance.GetCurrentInstant()
                         CorrelationId = cid
                     }
                     |> Common.Command)

            let sendMail (tod, text) =
                        ({
                             Command = EmailService.SendEmail(tod,text)
                             CreationDate = clockInstance.GetCurrentInstant()
                             CorrelationId = cid
                         }
                         |> Common.Command)

            let userActor =
                    mailbox.Self.Path.Name
                    |> SagaStarter.toOriginatorName
                    |> User.factory toEvent actorApi
            actor {
                let! msg = mailbox.Receive()
                match msg, state with
                | SagaStarter.SubscrptionAcknowledged mailbox _, _ ->
                    // notify saga starter about the subscription completed
                    match state with
                    | NotStarted -> return! WaitingForVerification |> StateChanged |> box |> Persist
                    | WaitingForVerification -> return! set state
                    | Completed ->
                        mailbox.Parent()
                            <! Passivate(Actor.PoisonPill.Instance)
                        return! set state
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
                    | StateChanged Completed ->
                        mailbox.Parent()
                        <! Passivate(Actor.PoisonPill.Instance)
                        return! set state
                    | _ -> return! set state
                | :? (Common.Event<User.Event>) as userEvent, _ ->
                    match userEvent with
                    | { Event = User.RegistrationRequested(_) } ->
                        mediator
                        <! box (Send(EmailService.EmailServicePath,
                                    EmailService.SendEmail("1" |> ShortString.ofString ,"2" |> LongString.ofString)))
                        return! set state
                    | { Event = User.VerificationSuccessful } ->
                        return! Completed |> StateChanged |> box |> Persist
                    | _ -> return! set state

                | :? (EmailService.Event) as emailEvent, _ ->
                    match emailEvent with
                    | EmailService.EmailSent ->
                        userActor <! waitForVerification()
                        return! set state
                | _ -> return! set state
            }
        set NotStarted
    let init toEvent (actorApi: IActor) (clock:IClock) =
        (AkklingHelpers.entityFactoryFor actorApi.System shardResolver "UserVerificationSaga"
             <| propsPersist (actorProp toEvent actorApi clock (typed actorApi.Mediator))
             <| true)
    let factory toEvent actorApi clock entityId =
        (init toEvent actorApi clock).RefFor DEFAULT_SHARD entityId

