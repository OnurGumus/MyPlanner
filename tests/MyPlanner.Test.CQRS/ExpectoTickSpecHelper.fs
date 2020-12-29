module ExpectoTickSpecHelper

open System.Reflection
open Expecto

let assembly = Assembly.GetExecutingAssembly()
let stepDefinitions = TickSpec.StepDefinitions(assembly)

let featureFromEmbeddedResource (resourceName: string): TickSpec.Feature =
    let stream =
        assembly.GetManifestResourceStream(resourceName)

    stepDefinitions.GenerateFeature(resourceName, stream)

let testListFromFeature (feature: TickSpec.Feature): Expecto.Test =
    feature.Scenarios
    |> Seq.map (fun scenario ->
        let testCaseF =
            if scenario.Name.Replace("Scenario:", "").Trim().StartsWith("_")
            then ftestCase
            else testCase

        testCaseF scenario.Name (fun () -> scenario.Action.Invoke())) // )
    |> Seq.toList
    |> (fun tests ->
        let testListF =
            if feature.Name.StartsWith("_") then ftestList else testList

        testListF feature.Name tests)
    |> testSequenced

let featureTest (resourceName: string) =
    (assembly.GetName().Name + "." + resourceName)
    |> featureFromEmbeddedResource
    |> testListFromFeature


