[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module MyPlanner.Shared.Domain

open Fable.Validation.Core

#if !FABLE
open Newtonsoft.Json
#endif
[<AutoOpen>]
module Common =

    let forceCreate v tryCreate =
        match tryCreate v with
        | Ok r -> r
        | Error (e: string list) -> invalidOp (System.String.Join(",", e))

#if !FABLE
    [<JsonObject(MemberSerialization = MemberSerialization.Fields)>]
#endif
    type ShortString = private ShortString of string

    module ShortString =
        let ofStringToResult (str: string) =
            single
                (fun t ->
                    t.TestOne str
                    |> t.MinLen 1 "must be at least 1 character"
                    |> t.MaxLen 200 "must be less than 200 characters"
                    |> t.Map ShortString
                    |> t.End)

        let ofStringToOption (str: string) =
            match str |> ofStringToResult with
            | Ok e -> Some e
            | _ -> None

        let ofString (str: string) = forceCreate str ofStringToResult

        let value_ =
            (fun (ShortString v) -> v), (fun v _ -> v |> ofString)


    let (|ShortString|) (ShortString str) = str

#if !FABLE
    [<JsonObject(MemberSerialization = MemberSerialization.Fields)>]
#endif
    type LongString = private LongString of string

    module LongString =
        let ofStringToResult (str: string) =
            single
                (fun t ->
                    t.TestOne str
                    |> t.MinLen 1 "must be at least 1 character"
                    |> t.Map LongString
                    |> t.End)

        let ofStringToOption (str: string) =
            match str |> ofStringToResult with
            | Ok e -> Some e
            | _ -> None

        let ofString (str: string) = forceCreate str ofStringToResult

        let value_ =
            (fun (LongString v) -> v), (fun v _ -> v |> ofString)


    let (|LongString|) (LongString str) = str


type Version = Version of int64

let version0 = Version 0L

module Version =
    let value_ =
        (fun (Version v) -> v), (fun v _ -> Version v)

type UserId = UserId of ShortString

module UserId =
    let value_ =
        (fun (UserId v) -> v), (fun v _ -> UserId v)

type UserName = UserName of ShortString

module UserName =
    let value_ =
        (fun (UserName v) -> v), (fun v _ -> UserName v)


type Password = Password of ShortString

module Password =
    let value_ =
        (fun (Password v) -> v), (fun v _ -> Password v)

type VerificationCode = VerificationCode of int
type User =
    {
        Id: UserId
        Version: Version
        UserName: UserName
        Password: Password
    }

module Command =
    type RegisterUser = User -> Result<unit, string> Async
