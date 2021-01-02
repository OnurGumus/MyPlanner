module Tests

open Expecto
open ExpectoTickSpecHelper

[<Tests>]
let show_tasks = featureTest "create-tasks.feature"
