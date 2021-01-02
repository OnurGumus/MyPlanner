module MyPlanner.Command.API

open MyPlanner.Shared.Domain.Command
open Common
open Domain.Task
open Serilog

open MyPlanner.Shared.Domain
open Domain
open Actor

let createTask (domainApi:IDomain) : CreateTask =
    fun ({Id = TaskId taskId} as task) ->
        async {
            let taskId = sprintf "task_%s" <| taskId

            let corID = taskId |> SagaStarter.toCid
            let taskActor =  domainApi.TaskFactory taskId

            let commonCommand: Command<_> =
                { Command = CreateTask task
                  CreationDate = Domain.clockInstance.GetCurrentInstant()
                  CorrelationId = corID }

            let c =
                { Cmd = commonCommand
                  EntityRef = taskActor
                  Filter =
                      (function
                      | TaskCreated _ -> true ) }
                |> Execute

            match! (domainApi.ActorApi.SubscribeForCommand c) with
            | { Event = TaskCreated t; Version = v } -> return Ok t
        }
[<Interface>]
type IAPI = 
    abstract member CreateTask : CreateTask
    abstract member ActorApi : IActor
    
let api config = 
    let actorApi = Actor.api config
    let domainApi = Domain.api actorApi
    { new IAPI with 
        member _.ActorApi = actorApi
        member _.CreateTask = 
            createTask domainApi }
