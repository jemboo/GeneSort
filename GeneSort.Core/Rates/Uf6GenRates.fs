namespace GeneSort.Core

open System
open FSharp.UMX

[<Struct; CustomEquality; NoComparison>]
type uf6GenRates = 
    private { 
        order: int
        seedGenRatesUf6: seed6GenRates
        opsGenRatesArray: opsGenRatesArray
    }

    static member create (order: int) (seedGenRatesUf6: seed6GenRates) (opsGenRatesArray: opsGenRatesArray) : uf6GenRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        { order = order; seedGenRatesUf6 = seedGenRatesUf6; opsGenRatesArray = opsGenRatesArray }

    member this.Order with get() = this.order
    member this.SeedGenRatesUf6 with get() = this.seedGenRatesUf6
    member this.OpsGenRatesArray with get() = this.opsGenRatesArray
    override this.GetHashCode() = 
        // 1. Extract the already-stable hashes from your individual component fields
        let h1 = this.seedGenRatesUf6.GetHashCode()
        let h2 = this.opsGenRatesArray.GetHashCode()
        // 2. Combine them using the exact same deterministic Knuth-style multiplier algorithm
        let mutable hash = 17
        hash <- hash * 23 + h1
        hash <- hash * 23 + h2
        hash

    override this.Equals(obj) = 
        match obj with
        | :? uf6GenRates as other -> 
            this.order = other.order &&
            this.seedGenRatesUf6.Equals(other.seedGenRatesUf6) &&
            this.opsGenRatesArray.Equals(other.opsGenRatesArray)
        | _ -> false


module Uf6GenRates =

    let makeUniform (order: int) : uf6GenRates =
        let genRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> opsGenRates.createUniform())
        uf6GenRates.create 
            order 
            (seed6GenRates.createUniform())
            (opsGenRatesArray.create genRatesArray)

    let biasTowards (order: int) (opsGenMode: opsGenMode) (biasAmt: float) : uf6GenRates =
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 6)
        if genRatesBaseArrayLength = 0 then
            uf6GenRates.create 
                order 
                (seed6GenRates.createUniform())
                (opsGenRatesArray.create [||])
        else
            let genRatesBaseArray =
                if genRatesBaseArrayLength = 1 then
                    [||]
                else
                    Array.init (genRatesBaseArrayLength - 1) (fun _ -> opsGenRates.createUniform())
            let lastGenRates = opsGenRates.createBiased(opsGenMode, biasAmt)
            let genRatesArray = Array.append genRatesBaseArray [|lastGenRates|]
            uf6GenRates.create 
                order 
                (seed6GenRates.createUniform())
                (opsGenRatesArray.create genRatesArray)