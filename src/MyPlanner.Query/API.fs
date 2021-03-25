module MyPlanner.Query.API

open MyPlanner.Query.Projection
open MyPlanner.Shared.Domain
open Microsoft.Extensions.Configuration
open MyPlanner.Shared
open Akka.Streams.Dsl
open Akka.Persistence.Query
open Akka.Streams
open Akkling.Streams

[<Interface>]
type IAPI =
    abstract Query<'t> :
        ?filter: string * ?orderby: string * ?thenby: string * ?take: int * ?skip: int ->
        list<'t> Async

    abstract Subscribe : (DataEvent -> unit) -> IKillSwitch

let subscribeToStream source mat sink =
    source
    |> Source.viaMat KillSwitch.single Keep.right
    |> Source.toMat (sink) Keep.both
    |> Graph.run mat

let api (config: IConfiguration) actorApi =
    let connString =
        config
            .GetSection(
                Constants.ConnectionString
            )
            .Value

    let source = Projection.init connString actorApi

    let subscribeCmd =

        (fun cb ->
            let sink = Sink.forEach (fun event -> cb (event))

            let ks, _ =
                subscribeToStream source actorApi.Materializer sink

            ks :> IKillSwitch)

    { new IAPI with
        override this.Subscribe(cb) = subscribeCmd (cb)

        override this.Query(?filter, ?orderby, ?thenby, ?take, ?skip) : Async<'t list> =
            let ctx = Sql.GetDataContext(connString)
            let tasks = ctx.Main.Tasks |> Seq.toArray

            let res =
                [
                    for task in tasks do
                        {
                            Id = (TaskId(ShortString.ofString task.Id))
                            Version = Version(task.Version)
                            Title = TaskTitle(ShortString.ofString task.Title)
                            Description = TaskDescription(LongString.ofString task.Description)
                        }
                ]
                |> box
                :?> list<'t>

            async { return res }
    }
