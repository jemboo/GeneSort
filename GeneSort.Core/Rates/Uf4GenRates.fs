namespace GeneSort.Core

type Uf4GenRates =
    {
        order: int
        seedOpsGenRates: OpsGenRates
        opsGenRatesArray: OpsGenRatesArray
    }

module Uf4GenRates =

    let makeUniform (order: int) : Uf4GenRates =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let genRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> OpsGenRates.createUniform())
        { 
            order = order
            seedOpsGenRates = OpsGenRates.createUniform()
            opsGenRatesArray = OpsGenRatesArray.create genRatesArray
        }

    let biasTowards (order: int) (opsGenMode: OpsGenMode) (biasAmt: float) : Uf4GenRates =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 4)
        if genRatesBaseArrayLength = 0 then
            {
                order = order
                seedOpsGenRates = OpsGenRates.createUniform()
                opsGenRatesArray = OpsGenRatesArray.create [||]
            }
        else
            let genRatesBaseArray =
                if genRatesBaseArrayLength = 1 then
                    [||]
                else
                    Array.init (genRatesBaseArrayLength - 1) (fun _ -> OpsGenRates.createUniform())

            let lastGenRates = OpsGenRates.createBiased(opsGenMode, biasAmt)
            let genRatesArray = Array.append genRatesBaseArray [|lastGenRates|]
            { 
                order = order
                seedOpsGenRates = OpsGenRates.createUniform()
                opsGenRatesArray = OpsGenRatesArray.create genRatesArray
            }
