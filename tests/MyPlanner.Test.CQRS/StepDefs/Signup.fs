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

    let emailResult : (ShortString * LongString) ref =  ref Unchecked.defaultof<_>

    let sendEmail x y = async { 
            emailResult := (x,y); 
            return ()
        }


    let commandApi =
        MyPlanner.Command.API.api config NodaTime.SystemClock.Instance sendEmail

    let queryApi =
        MyPlanner.Query.API.api config commandApi.ActorApi

    conn, commandApi, queryApi, emailResult


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
let ``I should receive verification code`` (result: Result<unit, string>, mail : (ShortString * LongString) ref) = 
        "Email not verified" |> Expect.wantOk result
        let (x,y)= (!mail)
        printfn "%A" x,y

