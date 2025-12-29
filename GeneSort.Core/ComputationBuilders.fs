namespace GeneSort.Core

open System.Threading

[<AutoOpen>]
module ComputationBuilders =

    /// Helper to bridge CancellationTokens into the AsyncResult flow
    let checkCancellation (token: CancellationToken) =
        async {
            if token.IsCancellationRequested then 
                return Error "Operation cancelled"
            else 
                return Ok ()
        }

    // --- Maybe Builder (Option) ---
    type MaybeBuilder() =
        member _.Bind(x, f) = Option.bind f x
        member _.Return(x) = Some x
        member _.ReturnFrom(x) = x
        member _.Zero() = None

    let maybe = MaybeBuilder()

    type AsyncResultBuilder() =
        member _.Bind(x: Async<Result<'T, 'E>>, f: 'T -> Async<Result<'U, 'E>>) =
            async {
                let! res = x
                match res with
                | Error e -> return Error e
                | Ok v -> return! f v
            }

        member _.Bind(x: Result<'T, 'E>, f: 'T -> Async<Result<'U, 'E>>) =
            match x with
            | Error e -> async { return Error e }
            | Ok v -> f v

        member _.Return(v) = async { return Ok v }
        
        // 1. This handles: return! async { return Ok 1 }
        member _.ReturnFrom(x: Async<Result<'T, 'E>>) = x
        
        // 2. This handles: return! Ok 1
        member _.ReturnFrom(x: Result<'T, 'E>) = async { return x }
    
        // --- Error Handling Support ---
    
        /// Handles 'try ... with'
        member _.TryWith(computation: Async<Result<'T, 'E>>, handler: exn -> Async<Result<'T, 'E>>) =
            async.TryWith(computation, handler)

        /// Handles 'try ... finally'
        member _.TryFinally(computation: Async<Result<'T, 'E>>, compensation: unit -> unit) =
            async.TryFinally(computation, compensation)

        /// Handles 'using' (disposable objects)
        member this.Using(resource: #System.IDisposable, binder: #System.IDisposable -> Async<Result<'V, 'E>>) =
            this.TryFinally(binder resource, (fun () -> if not (isNull resource) then resource.Dispose()))

        member _.Delay(f) = async.Delay(f)
        member _.Run(f) = f

    let asyncResult = AsyncResultBuilder()



    // --- AsyncOption Builder ---
    type AsyncOptionBuilder() =
        member _.Bind(x: Async<option<'T>>, f: 'T -> Async<option<'U>>) =
            async {
                let! res = x
                match res with
                | None -> return None
                | Some v -> return! f v
            }
        
        // IMPROVEMENT: Allow let! on plain Options
        member _.Bind(x: option<'T>, f: 'T -> Async<option<'U>>) =
            match x with
            | None -> async { return None }
            | Some v -> f v

        member _.Return(v) = async { return Some v }
        member _.ReturnFrom(x) = x

    let asyncOption = AsyncOptionBuilder()

[<RequireQualifiedAccess>]
module AsyncResult =
    let mapError (f: 'E -> 'F) (x: Async<Result<'T, 'E>>) : Async<Result<'T, 'F>> =
        async {
            let! res = x
            return Result.mapError f res
        }

    let ofAsync (x: Async<'T>) : Async<Result<'T, 'E>> =
        async {
            let! res = x
            return Ok res
        }


[<AutoOpen>]
module AsyncResultExtensions =
    let inline asAsync (res: Result<'T, 'E>) : Async<Result<'T, 'E>> = 
            async { return res }



// Extending the built-in Result module
module Result =
    let ofOption errorMsg = function
        | Some x -> Ok x
        | None   -> Error errorMsg