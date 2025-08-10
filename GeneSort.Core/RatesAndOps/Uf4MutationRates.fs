namespace GeneSort.Core


type Uf4MutationRates =
    {
        order:int;
        seedOpsTransitionRates: OpsTransitionRates;
        twoOrbitPairOpsTransitionRates: OpsTransitionRates list
    }

module Uf4MutationRates =

    let makeUniform (order:int) (perm_RsMutationRate:float) (twoOrbitMutationRate:float) =
        let mutRatesListLength = MathUtils.exactLog2 (order / 4)
        let mutRatesList = List.init mutRatesListLength (fun _ -> OpsTransitionRates.createUniform(twoOrbitMutationRate))
        { Uf4MutationRates.order = order;
          seedOpsTransitionRates = OpsTransitionRates.createUniform(perm_RsMutationRate);
          twoOrbitPairOpsTransitionRates = mutRatesList; }

    let biasTowards (order:int) (perm_RsMutationRate:float) (twoOrbitType:TwoOrbitType)  (baseAmt:float) (biasAmt:float) =
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 4) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> OpsTransitionRates.createUniform(baseAmt))
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        { Uf4MutationRates.order = order;
          seedOpsTransitionRates = OpsTransitionRates.createUniform(perm_RsMutationRate);
          twoOrbitPairOpsTransitionRates = genRatesListWithLast; }
