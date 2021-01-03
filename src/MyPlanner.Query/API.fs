module MyPlanner.Query.API

[<Interface>]
type IAPI =
    abstract Query<'t> : filter:string option -> orderby : string option -> 
        thenby: string option -> take: int option -> skip : int option ->  list<'t> Async

let api  connection actorApi = 
    Projection.init connection actorApi

    { new IAPI with
        override this.Query(filter: string option) (orderby: string option) (thenby: string option) (take: int option) (skip: int option): Async<'t list> = 
            
            failwith "Not Implemented" }
 
       