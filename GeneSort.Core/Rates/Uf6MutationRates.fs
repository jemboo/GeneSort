namespace GeneSort.Core

type Uf6MutationRates =
    {
        order: int
        seed6TransitionRates: Seed6TransitionRates
        opsTransitionRates: OpsTransitionRatesArray
    }

module Uf6MutationRates =

    let makeUniform (order: int) (seed6MutationRates: float) (twoOrbitMutationRate: float) 
         : Uf6MutationRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> OpsTransitionRates.createUniform(twoOrbitMutationRate))
        { Uf6MutationRates.order = order
          seed6TransitionRates = Seed6TransitionRates.createUniform(seed6MutationRates)
          opsTransitionRates = OpsTransitionRatesArray.create mutRatesArray }

    let biasTowards (order: int) (seed6MutationRates: float) (twoOrbitType: TwoOrbitType) (baseAmt: float) (biasAmt: float) 
         : Uf6MutationRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 6) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> OpsTransitionRates.createUniform(baseAmt))
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        { Uf6MutationRates.order = order
          seed6TransitionRates = Seed6TransitionRates.createUniform(seed6MutationRates)
          opsTransitionRates = OpsTransitionRatesArray.create (genRatesListWithLast |> List.toArray) }