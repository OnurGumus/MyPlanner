module MyPlanner.Command.Domain.EmailService

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
let EmailService = "EmailService"

[<Literal>]
let EmailServicePath = "/user/EmailService"

type Command = SendEmail of ShortString * LongString

type Event =
    | EmailSent
    | EmailFailed


let actorProp sendEmail (mailbox: Actor<_>) =
    let rec set (state: unit) =

        actor {
            match! mailbox.Receive() with
            | SendEmail (target, text) ->
                let sender = untyped <| mailbox.Sender()
                printf "%A" text
                Async.StartWithContinuations ( (sendEmail target text), (fun _ -> typed sender <! (EmailSent)), ignore, ignore)
                return! set ()
            | _ -> return! Unhandled

        }

    set ()

let init system mediator sendEmail =
    let emailService =
        spawn system <| EmailService <| props (actorProp sendEmail)

    typed mediator <! (emailService |> untyped |> Put)
