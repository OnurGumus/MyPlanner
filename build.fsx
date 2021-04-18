open System.Threading
#r "paket: groupref Build //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO

Target.initEnvironment ()

let serverPath =
    Path.getFullName "./src/MyPlanner.Server"

let automationPath =
    Path.getFullName "./tests/MyPlanner.Automation"

let serverTestPath =
    Path.getFullName "./tests/MyPlanner.Test"

let cqrsServerTestPath =
    Path.getFullName "./tests/MyPlanner.Test.CQRS"

let clientPath =
    Path.getFullName "./src/MyPlanner.Client"

let clientDeployPath = Path.combine clientPath "deploy"
let deployDir = Path.getFullName "./deploy"

let runTool procStart cmd args workingDir =
    let arguments =
        args |> String.split ' ' |> Arguments.OfArgs

    RawCommand(cmd, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> procStart
    |> ignore

let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""

    if result.ExitCode <> 0
    then failwithf "'dotnet %s' failed in %s" cmd workingDir

let openBrowser url =
    //https://github.com/dotnet/corefx/issues/10361
    ShellCommand url
    |> CreateProcess.fromCommand
    |> CreateProcess.ensureExitCodeWithMessage "opening browser failed"
    |> Proc.run
    |> ignore

Target.create "Clean" (fun _ -> [ deployDir; clientDeployPath ] |> Shell.cleanDirs)

Target.create "BuildServer" (fun _ -> runDotNet "build" serverPath)

Target.create
    "BuildServerOnlyRelease"
    (fun _ ->
        Shell.cleanDir deployDir

        runDotNet
            ("publish --configuration release --output "
             + deployDir)
            serverPath)
Target.create
    "RunCQRSTests"
    (fun _ ->
        runDotNet
            "test /p:AltCover=true /p:AltCoverShowSummary=YELLOW /p:AltCoverAttributeFilter=ExcludeFromCodeCoverage \
            /p:AltCoverForce=true /p:AltCoverLocalSource=true \
            /p:AltCoverAssemblyFilter=\"\.Test\" \
            /p:AltCoverVisibleBranches=true \
            /p:AltCoverTypeFilter=\"StartupCode|@\""
            cqrsServerTestPath)

Target.create
    "RunTests"
    (fun _ ->
        runDotNet
            "test /p:AltCover=true /p:AltCoverShowSummary=YELLOW /p:AltCoverAttributeFilter=ExcludeFromCodeCoverage \
            /p:AltCoverForce=true /p:AltCoverLocalSource=true \
            /p:AltCoverAssemblyFilter=\"\.Test|\.Command|\.Query\" \
            /p:AltCoverVisibleBranches=true \
            /p:AltCoverTypeFilter=\"StartupCode|@\" "
            serverTestPath)

Target.create "BuildRelease" (fun _ ->
    runDotNet "fable ./src/MyPlanner.Client.View -o  ./src/MyPlanner.Client.View/fable-output  --run yarn prod" __SOURCE_DIRECTORY__
)

Target.create "BuildDevClient" (fun _ ->
    runDotNet "fable ./src/MyPlanner.Client.View -o  ./src/MyPlanner.Client.View/fable-output" __SOURCE_DIRECTORY__
)

Target.create "WatchServer" (fun _ -> runDotNet "watch run" serverPath)

Target.create
    "Run"
    (fun _ ->
        let server =
            async { runDotNet "watch run" serverPath }

        let client =
            async {
                //let runTool = runTool Proc.run
                //runTool yarnTool ("webpack-dev-server --env.baseUrl=" + baseUrl) __SOURCE_DIRECTORY__
                runDotNet "fable watch ./src/MyPlanner.Client.View -s -o ./src/MyPlanner.Client.View/fable-output --run yarn dev" __SOURCE_DIRECTORY__
            }

        let browser =
            async {
                do! Async.Sleep 15000
                openBrowser "http://localhost:8080"
            }

        let vsCodeSession =
            Environment.hasEnvironVar "vsCodeSession"
        //don't remove this comment otherwise it doesn't work, LITERALLY doesn't work! :/
        let safeClientOnly =
            Environment.hasEnvironVar "safeClientOnly"

        let tasks =
            [ if not safeClientOnly then yield server
              yield client
              if not vsCodeSession then yield browser ]

        tasks
        |> Async.Parallel
        |> Async.RunSynchronously
        |> ignore)

Target.create "RunAutomation" (fun _ -> 
    runDotNet "fable  ./src/MyPlanner.Client.View -o ./src/MyPlanner.Client.View/fable-output  --run yarn prod" __SOURCE_DIRECTORY__
    runDotNet "run" automationPath)

open Fake.Core.TargetOperators

"Clean" ==> "BuildServer" ==> "RunAutomation"
"Clean" ==> "BuildServerOnlyRelease" ==> "BuildRelease" 

Target.runOrDefaultWithArguments "Build"
