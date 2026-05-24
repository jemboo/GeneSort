namespace GeneSort.Core

[<Struct; CustomEquality; NoComparison>]
type uf6MutationRates = 
    private { 
        order: int
        seed6TransitionRates: seed6TransitionRates
        opsTransitionRates: opsTransitionRatesArray
    }

    static member create 
            (order: int) 
            (seed6TransitionRates: seed6TransitionRates) 
            (opsTransitionRates: opsTransitionRatesArray) : uf6MutationRates =
        if order < 6 || order % 6 <> 0 then
            failwith $"Order must be at least 6 and divisible by 6, got {order}"
        { order = order; seed6TransitionRates = seed6TransitionRates; opsTransitionRates = opsTransitionRates }

    member this.Order with get() = this.order
    member this.Seed6TransitionRates with get() = this.seed6TransitionRates
    member this.OpsTransitionRates with get() = this.opsTransitionRates

    override this.Equals(other: obj) =
        match other with
        | :? uf6MutationRates as otherRates ->
            this.order = otherRates.order &&
            this.seed6TransitionRates = otherRates.seed6TransitionRates &&
            this.opsTransitionRates = otherRates.opsTransitionRates
        | _ -> false

    override this.GetHashCode() =
        // 1. Extract the already-stable hashes from your individual component fields
        let h1 = this.seed6TransitionRates.GetHashCode()
        let h2 = this.opsTransitionRates.GetHashCode()
        // 2. Combine them using the exact same deterministic Knuth-style multiplier algorithm
        let mutable hash = 17
        hash <- hash * 23 + h1
        hash <- hash * 23 + h2
        hash


module Uf6MutationRates =

    let makeUniform (order: int) (seed6MutationRates: float) (twoOrbitMutationRate: float) 
         : uf6MutationRates =
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 6)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> opsTransitionRates.createUniform(twoOrbitMutationRate))
        uf6MutationRates.create 
            order 
            (seed6TransitionRates.createUniform(seed6MutationRates))
            (opsTransitionRatesArray.create mutRatesArray)

    let biasTowards (order: int) (seed6MutationRates: float) (twoOrbitType: TwoOrbitType) (baseAmt: float) (biasAmt: float) 
         : uf6MutationRates =
        let mutRatesBaseListLength = MathUtils.exactLog2 (order / 6) - 1
        let mutRatesBaseList = List.init mutRatesBaseListLength (fun _ -> opsTransitionRates.createUniform(baseAmt))
        let lastGenRates = opsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesListWithLast = mutRatesBaseList @ [lastGenRates]
        uf6MutationRates.create 
            order 
            (seed6TransitionRates.createUniform(seed6MutationRates))
            (opsTransitionRatesArray.create (genRatesListWithLast |> List.toArray))