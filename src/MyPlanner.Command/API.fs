module MyPlanner.Command.API

open MyPlanner.Shared.Domain.Command
open Common
open Domain.User
open Serilog

open MyPlanner.Shared.Domain
open Domain.Api
open Actor
open NodaTime
open System
open User

let registerUser (domainApi: IDomain) : RegisterUser =
    fun user ->
        match user with
        | { Id = UserId (ShortString userId) } as user ->
            async {
                let userId = $"user_{userId}" |> Uri.EscapeUriString

                let corID = userId |> SagaStarter.toNewCid
                let taskActor = domainApi.UserFactory userId

                let commonCommand : Command<_> =
                    {
                        Command = Register user
                        CreationDate = domainApi.Clock.GetCurrentInstant()
                        CorrelationId = corID
                    }

                let c =
                    {
                        Cmd = commonCommand
                        EntityRef = taskActor
                        Filter =
                            (function
                            | VerificationRequested  -> true | _ -> false)
                    }
                    |> Execute

                match! (domainApi.ActorApi.SubscribeForCommand c) with
                | {
                      Event = (VerificationRequested)
                  } -> return Ok ()

                | _ -> return failwith "not supported"
            }

[<Interface>]
type IAPI =
    abstract ReisterUser : RegisterUser
    abstract ActorApi : IActor


let api config (clock: IClock) sendMail =
    let actorApi = Actor.api config
    let domainApi = Domain.Api.api clock actorApi sendMail

    { new IAPI with
        member _.ActorApi = actorApi
        member _.ReisterUser = registerUser domainApi
    }
