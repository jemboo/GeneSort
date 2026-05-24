namespace GeneSort.Core

open System
open FSharp.UMX

[<Struct; CustomEquality; NoComparison>]
type uf4GenRates = 
    private { 
        order: int
        seedOpsGenRates: opsGenRates
        opsGenRatesArray: opsGenRatesArray
    }

    static member create (order: int) (seedOpsGenRates: opsGenRates) (opsGenRatesArray: opsGenRatesArray) : uf4GenRates =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        { order = order; seedOpsGenRates = seedOpsGenRates; opsGenRatesArray = opsGenRatesArray }

    member this.Order with get() = this.order
    member this.SeedOpsGenRates with get() = this.seedOpsGenRates
    member this.OpsGenRatesArray with get() = this.opsGenRatesArray

    override this.GetHashCode() = 
        // 1. Extract the already-stable hashes from your individual component fields
        let h1 = this.seedOpsGenRates.GetHashCode()
        let h2 = this.opsGenRatesArray.GetHashCode()
        // 2. Combine them using the exact same deterministic Knuth-style multiplier algorithm
        let mutable hash = 17
        hash <- hash * 23 + h1
        hash <- hash * 23 + h2
        hash

    override this.Equals(obj) = 
        match obj with
        | :? uf4GenRates as other -> 
            this.order = other.order &&
            this.seedOpsGenRates.Equals(other.seedOpsGenRates) &&
            this.opsGenRatesArray.Equals(other.opsGenRatesArray)
        | _ -> false


module Uf4GenRates =

    let makeUniform (order: int) : uf4GenRates =
        let genRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> opsGenRates.createUniform())
        uf4GenRates.create 
            order 
            (opsGenRates.createUniform())
            (opsGenRatesArray.create genRatesArray)

    let biasTowards (order: int) (opsGenMode: opsGenMode) (biasAmt: float) : uf4GenRates =
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 4)
        if genRatesBaseArrayLength = 0 then
            uf4GenRates.create 
                order 
                (opsGenRates.createUniform())
                (opsGenRatesArray.create [||])
        else
            let genRatesBaseArray =
                if genRatesBaseArrayLength = 1 then
                    [||]
                else
                    Array.init (genRatesBaseArrayLength - 1) (fun _ -> opsGenRates.createUniform())
            let lastGenRates = opsGenRates.createBiased(opsGenMode, biasAmt)
            let genRatesArray = Array.append genRatesBaseArray [|lastGenRates|]
            uf4GenRates.create 
                order 
                (opsGenRates.createUniform())
                (opsGenRatesArray.create genRatesArray)