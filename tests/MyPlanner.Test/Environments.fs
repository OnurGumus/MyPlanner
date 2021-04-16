module MyPlanner.Test.Environments

open MyPlanner.Server.Query
open MyPlanner.Shared.Domain
open MyPlanner.Server.Command
open Microsoft.Extensions.Configuration

type AppEnv(config: IConfiguration) =

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

   