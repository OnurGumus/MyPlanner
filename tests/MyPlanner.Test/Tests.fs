module Tests

open Expecto
open ExpectoTickSpecHelper

[<Tests>]
let createTasks = featureTest "create-tasks.feature"

[<Tests>]
let startPage = featureTest "start-page.feature"
