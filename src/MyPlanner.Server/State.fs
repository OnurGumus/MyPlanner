module MyPlanner.Server.State

open Elmish
open Microsoft.Extensions.Configuration
open MyPlanner.Query

    
[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) =

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

    interface ITaskQuery with
        member _.GetTasks: GetTasks =  failwith "not impl"