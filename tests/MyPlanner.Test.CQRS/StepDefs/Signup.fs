module MyPlanner.Test.CQRS.StepDefs.Signup

open TickSpec
open MyPlanner.Command.API
open MyPlanner.Shared.Domain
open System.Threading
open MyPlanner.Query
open System.IO
open Akkling
open Expecto
open Microsoft.Extensions.Configuration
open Hocon.Extensions.Configuration
open Akkling.Streams


[<Given>]
let ``I am not registered`` () =

    let configBuilder = ConfigurationBuilder()

    let config =
        configBuilder.AddHoconFile("config.hocon").Build()

    let conn =
        MyPlanner.Query.Projection.createTables (config)

    let commandApi =
        MyPlanner.Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        MyPlanner.Query.API.api config commandApi.ActorApi

    conn, commandApi, queryApi


[<When>]
let ``I sign up with my username and pass`` (api: IAPI, (qapi : MyPlanner.Query.API.IAPI)) = 
    let res =
        api.ReisterUser
            { Id = "testuser" |> ShortString.ofString |> UserId
              Version = version0
              UserName = UserName(ShortString.ofString "onur")
              Password = Password(ShortString.ofString "Pass") }
        |> Async.RunSynchronously
    res

[<Then>]
let ``I should receive verification code`` (result: Result<unit, string>) = 
        "Email not verified" |> Expect.wantOk result

