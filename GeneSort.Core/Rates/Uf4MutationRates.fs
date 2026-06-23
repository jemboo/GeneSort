namespace GeneSort.Core


[<Struct; CustomEquality; NoComparison>]
type uf4MutationRates =
    private
        {
            order: int
            seedOpsTransitionRates: opsTransitionRates
            twoOrbitPairOpsTransitionRates: opsTransitionRatesArray
        }

        static member create 
                (order: int) 
                (seedOpsTransitionRates: opsTransitionRates) 
                (twoOrbitPairOpsTransitionRates: opsTransitionRatesArray) : uf4MutationRates =
            if order < 4 || order % 4 <> 0 then
                failwith $"Order must be at least 4 and divisible by 4, got {order}"
            { 
                order = order; 
                seedOpsTransitionRates = seedOpsTransitionRates; 
                twoOrbitPairOpsTransitionRates = twoOrbitPairOpsTransitionRates }

        static member createUniform 
                            (order: int) 
                            (seedRates:opsTransitionRates) 
                            (rates:opsTransitionRates) : uf4MutationRates =
            let genRatesArrayLength = MathUtils.exactLog2 (order / 4)
            let genRatesArray = Array.init genRatesArrayLength (fun _ -> rates)
            uf4MutationRates.create 
                order
                seedRates
                (opsTransitionRatesArray.create genRatesArray)

        member this.Order with get() = this.order
        member this.SeedOpsTransitionRates with get() = this.seedOpsTransitionRates
        member this.TwoOrbitPairOpsTransitionRates with get() = this.twoOrbitPairOpsTransitionRates

        override this.GetHashCode() = 
            // 1. Extract the already-stable hashes from your individual component fields
            let h1 = this.seedOpsTransitionRates.GetHashCode()
            let h2 = this.twoOrbitPairOpsTransitionRates.GetHashCode()

            // 2. Combine them using the exact same deterministic Knuth-style multiplier algorithm
            let mutable hash = 17
            hash <- hash * 23 + h1
            hash <- hash * 23 + h2
            hash

        override this.Equals(obj) = 
            match obj with
            | :? uf4MutationRates as other -> 
                this.order = other.order &&
                this.seedOpsTransitionRates.Equals(other.seedOpsTransitionRates) &&
                this.twoOrbitPairOpsTransitionRates.Equals(other.twoOrbitPairOpsTransitionRates)
            | _ -> false


module Uf4MutationRates =

    let makeUniform (order: int) (seed4MutationRates: float) (twoOrbitMutationRate: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesArrayLength = MathUtils.exactLog2 (order / 4)
        let mutRatesArray = Array.init mutRatesArrayLength (fun _ -> opsTransitionRates.createUniform twoOrbitMutationRate)
        { uf4MutationRates.order = order
          seedOpsTransitionRates = opsTransitionRates.createUniform seed4MutationRates
          twoOrbitPairOpsTransitionRates = opsTransitionRatesArray.create mutRatesArray }

    let makeUniform2 
                    (order: int) 
                    (seedRates: opsTransitionRates) 
                    (twoOrbitRates: opsTransitionRates) :uf4MutationRates =
        uf4MutationRates.createUniform order seedRates twoOrbitRates


    let biasTowards (order: int) 
                    (perm_RsMutationRate: float) 
                    (twoOrbitType: twoOrbitType) 
                    (baseAmt: float) 
                    (biasAmt: float) =
        if order < 4 || order % 4 <> 0 then
            failwith $"Order must be at least 4 and divisible by 4, got {order}"
        let mutRatesBaseArrayLength = MathUtils.exactLog2 (order / 4) - 1
        let mutRatesBaseArray = Array.init mutRatesBaseArrayLength (fun _ -> opsTransitionRates.createUniform baseAmt)
        let lastGenRates = opsTransitionRates.createBiased twoOrbitType baseAmt biasAmt
        let genRatesArray = Array.append mutRatesBaseArray [|lastGenRates|]
        { uf4MutationRates.order = order
          seedOpsTransitionRates = opsTransitionRates.createUniform perm_RsMutationRate
          twoOrbitPairOpsTransitionRates = opsTransitionRatesArray.create genRatesArray }