module MyPlanner.Shared.Domain

open Fable.Validation.Core

[<AutoOpen>]
module Common =

    let forceCreate v tryCreate =
        match tryCreate v with
        | Ok r -> r
        | Error (e: string list) -> invalidOp (System.String.Join(",", e))

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

type TaskId = TaskId of ShortString

module TaskId =
    let value_ =
        (fun (TaskId v) -> v), (fun v _ -> TaskId v)

type TaskDescription = TaskDescription of LongString

module TaskDescription =
    let value_ =
        (fun (TaskDescription v) -> v), (fun v _ -> TaskDescription v)


type TaskTitle = TaskTitle of ShortString

module TaskTitle =
    let value_ =
        (fun (TaskTitle v) -> v), (fun v _ -> TaskTitle v)

type Task =
    {
        Id: TaskId
        Version: Version
        Title: TaskTitle
        Description: TaskDescription
    }

module Command =
    type CreateTask = Task -> Result<Task, string> Async
