module MyPlanner.Command.API

open MyPlanner.Shared.Domain.Command
open Common
open Domain.Task
open Serilog

Domain.init()

let createTask: CreateTask =
    fun task ->
        async {
            let taskId = sprintf "task_%s" <| task.Id.ToString()

            let corID = taskId |> SagaStarter.toCid
            let taskActor = factory taskId

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

            match! (Actor.subscribeForCommand c) with
            | { Event = TaskCreated t; Version = v } -> return Ok t
        }
