namespace GeneSort.Core


type uf4MutationRates =
    {
        order: int
        seedOpsTransitionRates: opsTransitionRates
        twoOrbitPairOpsTransitionRates: opsTransitionRatesArray
    }

module Uf4MutationRates =

    let makeUniform (order: int) (seed4MutationRates: float) (twoOrbitMutationRate: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> opsTransitionRates.createUniform twoOrbitMutationRate)
        { uf4MutationRates.order = order
          seedOpsTransitionRates = opsTransitionRates.createUniform seed4MutationRates
          twoOrbitPairOpsTransitionRates = opsTransitionRatesArray.create mutRatesArray }

    let biasTowards (order: int) (perm_RsMutationRate: float) (twoOrbitType: TwoOrbitType) (baseAmt: float) (biasAmt: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesBaseArrayLength = MathUtils.exactLog2 (order / 4) - 1
        let mutRatesBaseArray = Array.init mutRatesBaseArrayLength (fun _ -> opsTransitionRates.createUniform baseAmt)
        let lastGenRates = opsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesArray = Array.append mutRatesBaseArray [|lastGenRates|]
        { uf4MutationRates.order = order
          seedOpsTransitionRates = opsTransitionRates.createUniform perm_RsMutationRate
          twoOrbitPairOpsTransitionRates = opsTransitionRatesArray.create genRatesArray }