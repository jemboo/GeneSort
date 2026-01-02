namespace GeneSort.Core
open System
open FSharp.UMX


type OutputError = string

module MonadUtils =

    let getValueAsync (asyncResult: Async<Result<'T, OutputError>>) : Async<'T> =
        async {
            let! result = asyncResult
            match result with
            | Ok value -> return value
            | Error err -> return failwith err
        }

    let tryGetValueAsync (asyncResult: Async<Result<'T, OutputError>>) : Async<'T option> =
        async {
            let! result = asyncResult
            match result with
            | Ok value -> return Some value
            | Error _ -> return None
        }


    let getValueOrDefaultAsync (defaultValue: 'T) (asyncResult: Async<Result<'T, OutputError>>) : Async<'T> =
        async {
            let! result = asyncResult
            match result with
            | Ok value -> return value
            | Error _ -> return defaultValue
        }


    let getValue (asyncResult: Async<Result<'T, OutputError>>) : 'T =
        asyncResult
        |> Async.RunSynchronously
        |> function
            | Ok value -> value
            | Error err -> failwith err
    
    let tryGetValue (asyncResult: Async<Result<'T, OutputError>>) : 'T option =
        asyncResult
        |> Async.RunSynchronously
        |> function
            | Ok value -> Some value
            | Error _ -> None
    
    let getValueOrDefault (defaultValue: 'T) (asyncResult: Async<Result<'T, OutputError>>) : 'T =
        asyncResult
        |> Async.RunSynchronously
        |> function
            | Ok value -> value
            | Error _ -> defaultValue