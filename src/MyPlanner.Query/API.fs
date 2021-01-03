module MyPlanner.Query.API

open MyPlanner.Query.Projection
open MyPlanner.Shared.Domain

[<Interface>]
type IAPI =
    abstract Query<'t> : ?filter:string option
                         * ?orderby:string option
                         * ?thenby:string option
                         * ?take:int option
                         * ?skip:int option
                         -> list<'t> Async

let api connectionString actorApi =
    Projection.init connectionString actorApi
    { new IAPI with
        override this.Query(?filter, ?orderby, ?thenby, ?take, ?skip): Async<'t list> =
            let ctx = Sql.GetDataContext(connectionString)
            let tasks = ctx.Main.Tasks |> Seq.toArray

            let res =
                [ for task in tasks do
                    { Id = (TaskId task.Id)
                      Version = Version(task.Version) } ]
                |> box
                :?> list<'t>

            async { return res } }
