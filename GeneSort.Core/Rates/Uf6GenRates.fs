namespace GeneSort.Core

open System
open FSharp.UMX

type uf6GenRates = 
    private { 
        order: int
        seedGenRatesUf6: Seed6GenRates
        opsGenRatesArray: OpsGenRatesArray
    }

    static member create (order: int) (seedGenRatesUf6: Seed6GenRates) (opsGenRatesArray: OpsGenRatesArray) : uf6GenRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        { order = order; seedGenRatesUf6 = seedGenRatesUf6; opsGenRatesArray = opsGenRatesArray }

    member this.Order with get() = this.order
    member this.SeedGenRatesUf6 with get() = this.seedGenRatesUf6
    member this.OpsGenRatesArray with get() = this.opsGenRatesArray

module Uf6GenRates =

    let makeUniform (order: int) : uf6GenRates =
        let genRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> OpsGenRates.createUniform())
        uf6GenRates.create 
            order 
            (Seed6GenRates.createUniform())
            (OpsGenRatesArray.create genRatesArray)

    let biasTowards (order: int) (opsGenMode: OpsGenMode) (biasAmt: float) : uf6GenRates =
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 6)
        if genRatesBaseArrayLength = 0 then
            uf6GenRates.create 
                order 
                (Seed6GenRates.createUniform())
                (OpsGenRatesArray.create [||])
        else
            let genRatesBaseArray =
                if genRatesBaseArrayLength = 1 then
                    [||]
                else
                    Array.init (genRatesBaseArrayLength - 1) (fun _ -> OpsGenRates.createUniform())
            let lastGenRates = OpsGenRates.createBiased(opsGenMode, biasAmt)
            let genRatesArray = Array.append genRatesBaseArray [|lastGenRates|]
            uf6GenRates.create 
                order 
                (Seed6GenRates.createUniform())
                (OpsGenRatesArray.create genRatesArray)