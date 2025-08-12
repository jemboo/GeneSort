namespace GeneSort.Core

type Uf6GenRates =
    {
        order: int
        seedGenRatesUf6: Seed6GenRates
        opsGenRatesArray: OpsGenRatesArray
    }

module Uf6GenRates =

    let makeUniform (order: int) : Uf6GenRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        let genRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let genRatesArray = Array.init genRatesArrayLength (fun _ -> OpsGenRates.createUniform())
        { 
            order = order
            seedGenRatesUf6 = Seed6GenRates.createUniform()
            opsGenRatesArray = OpsGenRatesArray.create genRatesArray
        }

    let biasTowards (order: int) (opsGenMode: OpsGenMode) (biasAmt: float) : Uf6GenRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        let genRatesBaseArrayLength = MathUtils.exactLog2 (order / 6)
        if genRatesBaseArrayLength = 0 then
            {
                order = order
                seedGenRatesUf6 = Seed6GenRates.createUniform()
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
                seedGenRatesUf6 = Seed6GenRates.createUniform()
                opsGenRatesArray = OpsGenRatesArray.create genRatesArray
            }
