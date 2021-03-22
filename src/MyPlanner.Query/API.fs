module MyPlanner.Query.API

open MyPlanner.Query.Projection
open MyPlanner.Shared.Domain
open Microsoft.Extensions.Configuration
open MyPlanner.Shared

[<Interface>]
type IAPI =
    abstract Query<'t> : ?filter:string * ?orderby:string * ?thenby:string * ?take:int * ?skip:int -> list<'t> Async

let api (config: IConfiguration) actorApi =
    let connString =
        config
            .GetSection(Constants.ConnectionString)
            .Value

    Projection.init connString actorApi

    { new IAPI with
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
