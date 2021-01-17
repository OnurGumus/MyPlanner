module MyPlanner.Command.API

open MyPlanner.Shared.Domain.Command
open Common
open Domain.Task
open Serilog

open MyPlanner.Shared.Domain
open Domain
open Actor
open NodaTime
open System

let createTask (domainApi: IDomain): CreateTask =
    fun ({ Id = TaskId taskId } as task) ->
        async {
            let taskId = $"task_{taskId}" |> Uri.EscapeUriString

            let corID = taskId |> SagaStarter.toCid
            let taskActor = domainApi.TaskFactory taskId

            let commonCommand: Command<_> =
                { Command = CreateTask task
                  CreationDate = domainApi.Clock.GetCurrentInstant()
                  CorrelationId = corID }

            let c =
                { Cmd = commonCommand
                  EntityRef = taskActor
                  Filter =
                      (function
                      | TaskCreated _ -> true) }
                |> Execute

            match! (domainApi.ActorApi.SubscribeForCommand c) with
            | { Event = TaskCreated t; Version = v } -> return Ok t
        }

[<Interface>]
type IAPI =
    abstract CreateTask: CreateTask
    abstract ActorApi: IActor


let api config (clock: IClock) =
    let actorApi = Actor.api config
    let domainApi = Domain.api clock actorApi
    { new IAPI with
        member _.ActorApi = actorApi
        member _.CreateTask = createTask domainApi }
