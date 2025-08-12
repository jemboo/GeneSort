namespace GeneSort.Core


type Uf4MutationRates =
    {
        order: int
        seedOpsTransitionRates: OpsTransitionRates
        twoOrbitPairOpsTransitionRates: OpsTransitionRatesArray
    }

module Uf4MutationRates =

    let makeUniform (order: int) (perm_RsMutationRate: float) (twoOrbitMutationRate: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> OpsTransitionRates.createUniform twoOrbitMutationRate)
        { Uf4MutationRates.order = order
          seedOpsTransitionRates = OpsTransitionRates.createUniform perm_RsMutationRate
          twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create mutRatesArray }

    let biasTowards (order: int) (perm_RsMutationRate: float) (twoOrbitType: TwoOrbitType) (baseAmt: float) (biasAmt: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesBaseArrayLength = MathUtils.exactLog2 (order / 4) - 1
        let mutRatesBaseArray = Array.init mutRatesBaseArrayLength (fun _ -> OpsTransitionRates.createUniform baseAmt)
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesArray = Array.append mutRatesBaseArray [|lastGenRates|]
        { Uf4MutationRates.order = order
          seedOpsTransitionRates = OpsTransitionRates.createUniform perm_RsMutationRate
          twoOrbitPairOpsTransitionRates = OpsTransitionRatesArray.create genRatesArray }