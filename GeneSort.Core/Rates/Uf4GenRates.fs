namespace GeneSort.Core

open System
open FSharp.UMX

type uf4GenRates = 
    private { 
        order: int
        seedOpsGenRates: OpsGenRates
        opsGenRatesArray: OpsGenRatesArray
    }

    static member create (order: int) (seedOpsGenRates: OpsGenRates) (opsGenRatesArray: OpsGenRatesArray) : uf4GenRates =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        { order = order; seedOpsGenRates = seedOpsGenRates; opsGenRatesArray = opsGenRatesArray }

    member this.Order with get() = this.order
    member this.SeedOpsGenRates with get() = this.seedOpsGenRates
    member this.OpsGenRatesArray with get() = this.opsGenRatesArray

module Uf4GenRates =

    let makeUniform (order: int) : uf4GenRates =
        let genRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> OpsGenRates.createUniform())
        uf4GenRates.create 
            order 
            (OpsGenRates.createUniform())
            (OpsGenRatesArray.create genRatesArray)

    let biasTowards (order: int) (opsGenMode: OpsGenMode) (biasAmt: float) : uf4GenRates =
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 4)
        if genRatesBaseArrayLength = 0 then
            uf4GenRates.create 
                order 
                (OpsGenRates.createUniform())
                (OpsGenRatesArray.create [||])
        else
            let genRatesBaseArray =
                if genRatesBaseArrayLength = 1 then
                    [||]
                else
                    Array.init (genRatesBaseArrayLength - 1) (fun _ -> OpsGenRates.createUniform())
            let lastGenRates = OpsGenRates.createBiased(opsGenMode, biasAmt)
            let genRatesArray = Array.append genRatesBaseArray [|lastGenRates|]
            uf4GenRates.create 
                order 
                (OpsGenRates.createUniform())
                (OpsGenRatesArray.create genRatesArray)