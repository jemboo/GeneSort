namespace GeneSort.Project.Test

open System
open FSharp.UMX

open MessagePack.Resolvers
open MessagePack
open MessagePack.FSharp
open System.IO
open System.Threading
open GeneSort.Runs.Params
open GeneSort.Runs

type ProjectTests() =

    let resolver = CompositeResolver.Create(FSharpResolver.Instance, StandardResolver.Instance)
    let options = MessagePackSerializerOptions.Standard.WithResolver(resolver)

    // Helper to create a temporary directory for tests
    let createTempDir () =
        let tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        Directory.CreateDirectory tempPath |> ignore
        tempPath

    // Helper to clean up temporary directory with retries
    let cleanupTempDir (path: string) =
        if Directory.Exists path then
            let maxRetries = 5
            let retryDelayMs = 100
            let rec tryDelete retries =
                try
                    Directory.Delete(path, true)
                with
                | :? IOException when retries > 0 ->
                    Thread.Sleep retryDelayMs
                    tryDelete (retries - 1)
                | ex when retries = 0 ->
                    printfn "Failed to delete directory %s after %d retries: %s" path maxRetries ex.Message
            tryDelete maxRetries

    let paramMapRefiner (runParametersSet: runParameters seq) = 
        runParametersSet


    // Helper to create a sample project
    let createProject (parameterSpans: list<string * list<string>>) =

        let outputDataTypes = 
            [|
                outputDataType.TextReport ("Bins" |> UMX.tag<textReportName>); 
                outputDataType.TextReport ("Profiles" |> UMX.tag<textReportName>); 
                outputDataType.TextReport ("Report3" |> UMX.tag<textReportName>); 
                outputDataType.TextReport ("Report4" |> UMX.tag<textReportName>);
            |]

        Project.create
            ("TestProject"  |> UMX.tag<projectName>)
            "Test Description"
            parameterSpans
            3<replNumber>
            outputDataTypes