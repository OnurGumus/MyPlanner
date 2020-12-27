module Program

open Expecto.Tests

[<EntryPoint>]
let main args =
    runTestsInAssemblyWithCLIArgs [] [|
        "--sequenced"
    |]
