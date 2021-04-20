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
open Query
open MyPlanner.Shared
open Hocon.Extensions.Configuration




let publicPath (env:#IConfiguration) =  Path.GetFullPath env.[Constants.ClientPath]


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



let bridge env =
    Bridge.mkServer Constants.Socket_Endpoint State.init (State.update env)
    |> Bridge.run Giraffe.server

let webApp env: HttpHandler =
    choose [ bridge env
             GET >=> htmlFile (publicPath env  + "/index.html") ]

let root     appEnv: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->

        webApp appEnv next ctx


let configureApp config envFactory (app: IApplicationBuilder) =
    let appEnv = envFactory config
    app
        .UseDefaultFiles()
        .UseStaticFiles()
        .UseWebSockets()
        .UseGiraffe(root appEnv)

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

    services.AddSingleton<Serialization.Json.IJsonSerializer>(Thoth.Json.Giraffe.ThothSerializer())
    |> ignore
