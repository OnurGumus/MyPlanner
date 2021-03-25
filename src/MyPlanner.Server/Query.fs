module MyPlanner.Server.Query

open MyPlanner.Shared.Domain
open Akka.Streams.Dsl
open MyPlanner.Query.Projection
open Akka.Streams

[<Interface>]
type IQuery =
    abstract Query<'t> : ?filter:string * ?orderby:string * ?thenby:string * ?take:int * ?skip:int -> list<'t> Async
    abstract Source : Source<DataEvent,unit>
    abstract Mat : IMaterializer