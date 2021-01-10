module MyPlanner.Server.Query

open MyPlanner.Shared.Domain

[<Interface>]
type IQuery =
    abstract Query<'t> : ?filter:string
                         * ?orderby:string
                         * ?thenby:string
                         * ?take:int
                         * ?skip:int
                         -> list<'t> Async
