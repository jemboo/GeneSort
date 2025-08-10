namespace GeneSort.Core


type Uf4GenRates =
    {
        order:int;
        seedOpsGenRates: OpsGenRates;
        opsGenRatesList: OpsGenRates list
    }

module Uf4GenRates =

    let makeUniform (order:int) : Uf4GenRates =

        if order < 4 then
            failwith "TwoOrbitUfStep order must be at least 4"
        if order % 2 <> 0 then
            failwith "TwoOrbitUfStep order must be even"
        let genRatesListLength = MathUtils.exactLog2 (order / 4)
        let genRatesList = List.init genRatesListLength (fun _ -> OpsGenRates.createUniform())
        { 
            Uf4GenRates.order = order;
            seedOpsGenRates = OpsGenRates.createUniform();
            opsGenRatesList = genRatesList; 
        }


    let biasTowards (order:int) (opsGenMode:OpsGenMode) (biasAmt:float) 
                : Uf4GenRates =

        let genRatesBaseListLength = MathUtils.exactLog2 (order / 4)

        if (genRatesBaseListLength = 0)  then
            {
                Uf4GenRates.order = order;
                seedOpsGenRates = OpsGenRates.createUniform();
                opsGenRatesList = []
            }
        else
            let genRatesBaseList =
                if (genRatesBaseListLength = 1) 
                then []
                else List.init (genRatesBaseListLength - 1) (fun _ -> OpsGenRates.createUniform())

            let lastGenRates = OpsGenRates.createBiased(opsGenMode, biasAmt)

            let genRatesListWithLast = genRatesBaseList @ [lastGenRates]
            { 
                Uf4GenRates.order = order;
                seedOpsGenRates = OpsGenRates.createUniform();
                opsGenRatesList = genRatesListWithLast; 
            }
