[<System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
module MyPlanner.Server.Server

open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Http
open Serilog
open Elmish
open Elmish.Bridge
open MyPlanner.Server
open MyPlanner.Shared.Msg

#if DEBUG
let publicPath =
    Path.GetFullPath "../MyPlanner.Client/deploy"
#else
let publicPath = Path.GetFullPath "./clientFiles"
#endif


type ServerMsg =
    | Remote of ClientToServer.Msg
// type ElmishBridge.BridgeServer<'arg, 'model, 'server, 'client, 'impl> with
//     member this.AddSeriLog =
//         this.AddMsgLogging(fun m -> Log.Debug("New message: {Msg}", m))
//             .AddSocketRawMsgLogging(fun m -> Log.Debug("Remote message: {Msg}", m))
//             .AddModelLogging(fun m -> Log.Debug("Updated state: {State}", m))

// /// Connect the Elmish functions to an endpoint for websocket connections
// let bridge env: HttpHandler =
//     ElmishBridge.Bridge.mkServer
//     <| Socket_Endpoint
//     <| State.init
//     <| State.update env
//     |> fun program -> program.AddSeriLog
//     |> ElmishBridge.Bridge.run Elmish.Bridge.Giraffe.server

let init dispatch () = dispatch ServerToClient.ServerConnected, Cmd.none

let rec update env clientDispatch msg state = state, Cmd.none

[<Literal>]
let Socket_Endpoint = "/socket/main"

let bridge env =
    Bridge.mkServer Socket_Endpoint init (update env)
    |> Bridge.run Giraffe.server

let webApp env: HttpHandler =
    choose [
        bridge env
        GET >=> htmlFile (publicPath + "/index.html")
    ]

let root: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let config = ctx.GetService<IConfiguration>()

        let appEnv =
            State.AppEnv(config)

        webApp appEnv next ctx


let configureApp (app: IApplicationBuilder) =
    app.UseDefaultFiles().UseStaticFiles().UseWebSockets().UseGiraffe root

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

    services.AddSingleton<Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer())
    |> ignore
