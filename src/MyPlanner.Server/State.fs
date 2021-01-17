module MyPlanner.Server.State

open Microsoft.Extensions.Configuration
open Query
open MyPlanner.Shared

[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
type AppEnv(config: IConfiguration) =
    let connString =
        config.GetSection(Constants.ConnectionString)

    let commandApi =
        MyPlanner.Command.API.api config NodaTime.SystemClock.Instance

    let queryApi =
        MyPlanner.Query.API.api config commandApi.ActorApi

    interface IConfiguration with
        member _.Item
            with get (key: string) = config.[key]
            and set key v = config.[key] <- v

        member _.GetChildren() = config.GetChildren()
        member _.GetReloadToken() = config.GetReloadToken()
        member _.GetSection key = config.GetSection(key)

    interface IQuery with
        member _.Query(?filter, ?orderby, ?thenby, ?take, ?skip) =
            queryApi.Query(?filter = filter, ?orderby = orderby, ?thenby = thenby, ?take = take, ?skip = skip)
