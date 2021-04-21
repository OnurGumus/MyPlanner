module MyPlanner.Command.API

open MyPlanner.Shared.Domain.Command
open Common
open Domain.User
open Serilog

open MyPlanner.Shared.Domain
open Domain
open Actor
open NodaTime
open System

let registerUser (domainApi: IDomain) : RegisterUser =
    fun user ->
        match user with
        | { Id = UserId (ShortString userId) } as user ->
            async {
                let userId = $"user_{userId}" |> Uri.EscapeUriString

                let corID = userId |> SagaStarter.toCid
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
                            | VerificationRequested _ -> true)
                    }
                    |> Execute

                match! (domainApi.ActorApi.SubscribeForCommand c) with
                | { Event = (VerificationRequested (user,code)); Version = v } -> return Ok code
            }

[<Interface>]
type IAPI =
    abstract ReisterUser : RegisterUser
    abstract ActorApi : IActor


let api config (clock: IClock) =
    let actorApi = Actor.api config
    let domainApi = Domain.api clock actorApi

    { new IAPI with
        member _.ActorApi = actorApi
        member _.ReisterUser = registerUser domainApi
    }
