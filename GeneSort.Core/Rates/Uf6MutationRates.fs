﻿namespace GeneSort.Core

type uf6MutationRates = 
    private { 
        order: int
        seed6TransitionRates: Seed6TransitionRates
        opsTransitionRates: OpsTransitionRatesArray
    }

    static member create 
            (order: int) 
            (seed6TransitionRates: Seed6TransitionRates) 
            (opsTransitionRates: OpsTransitionRatesArray) : uf6MutationRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        { order = order; seed6TransitionRates = seed6TransitionRates; opsTransitionRates = opsTransitionRates }

    member this.Order with get() = this.order
    member this.Seed6TransitionRates with get() = this.seed6TransitionRates
    member this.OpsTransitionRates with get() = this.opsTransitionRates

module Uf6MutationRates =

    let makeUniform (order: int) (seed6MutationRates: float) (twoOrbitMutationRate: float) 
         : uf6MutationRates =
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> OpsTransitionRates.createUniform(twoOrbitMutationRate))
        uf6MutationRates.create 
            order 
            (Seed6TransitionRates.createUniform(seed6MutationRates))
            (OpsTransitionRatesArray.create mutRatesArray)

    let biasTowards (order: int) (seed6MutationRates: float) (twoOrbitType: TwoOrbitType) (baseAmt: float) (biasAmt: float) 
         : uf6MutationRates =
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 6) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> OpsTransitionRates.createUniform(baseAmt))
        let lastGenRates = OpsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        uf6MutationRates.create 
            order 
            (Seed6TransitionRates.createUniform(seed6MutationRates))
            (OpsTransitionRatesArray.create (genRatesListWithLast |> List.toArray))