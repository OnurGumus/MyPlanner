module MyPlanner.Query.API

open MyPlanner.Query.Projection
open MyPlanner.Shared.Domain
open Microsoft.Extensions.Configuration
open MyPlanner.Shared
open Akka.Streams.Dsl
open Akka.Persistence.Query

[<Interface>]
type IAPI =
    abstract Query<'t> : ?filter:string * ?orderby:string * ?thenby:string * ?take:int * ?skip:int -> list<'t> Async
    abstract Source : Source<Task,unit>

let api (config: IConfiguration) actorApi =
    let connString =
        config
            .GetSection(Constants.ConnectionString)
            .Value

    let source = Projection.init connString actorApi

    { new IAPI with
        override this.Source = source
        override this.Query(?filter, ?orderby, ?thenby, ?take, ?skip): Async<'t list> =
            let ctx = Sql.GetDataContext(connString)
            let tasks = ctx.Main.Tasks |> Seq.toArray

            let res =
                [ for task in tasks do
                    { Id = (TaskId (ShortString.ofString task.Id))
                      Version = Version(task.Version)
                      Title = TaskTitle (ShortString.ofString task.Title)
                      Description = TaskDescription(LongString.ofString task.Description) } ]
                |> box
                :?> list<'t>

            async { return res } }
