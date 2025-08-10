namespace GeneSort.Core


type Uf6GenRates =
    {
        order:int;
        seedGenRatesUf6: Seed6GenRates;
        opsGenRatesList: OpsGenRates list
    }

module Uf6GenRates =

    let makeUniform (order:int) =
        let genRatesListLength = MathUtils.exactLog2 (order / 6)
        let genRatesList = List.init genRatesListLength (fun _  -> OpsGenRates.createUniform())
        { Uf6GenRates.order = order;
          seedGenRatesUf6 = Seed6GenRates.createUniform();
          opsGenRatesList = genRatesList; }
           