namespace GeneSort.Core

module UnfolderOps4 = 

    // makeTwoOrbitUnfolder4
    let makeTwoOrbitUf4 (floatPicker:unit -> float) (uf4GenRates:Uf4GenRates)
                        : TwoOrbitUf4 =

        let seedTypeUf4 = floatPicker |> uf4GenRates.seedOpsGenRates.PickMode |> OpsGenMode.toTwoOrbitType
        let twoOrbitUnfolderSteps = 
            uf4GenRates.opsGenRatesArray
                |> Array.mapi(fun dex rates ->
                    TwoOrbitTypeOps.makeTwoOrbitUnfolderStep floatPicker (4 * (MathUtils.integerPower 2 dex)) rates)
        TwoOrbitUf4.create seedTypeUf4 twoOrbitUnfolderSteps


    // mutateTwoOrbitUnfolder4
    let mutateTwoOrbitUf4
                (floatPicker:unit -> float)
                (uf4MutationRates:Uf4MutationRates) 
                (twoOrbitUf4:TwoOrbitUf4) : TwoOrbitUf4 =

        let twoOrbitTypeMutated = 
                uf4MutationRates.seedOpsTransitionRates.TransitionMode 
                        floatPicker 
                        (twoOrbitUf4.TwoOrbitType |> OpsGenMode.fromTwoOrbitType )
                        |> OpsGenMode.toTwoOrbitType

        let twoOrbitUnfolderSteps = 
                uf4MutationRates.twoOrbitPairOpsTransitionRates.RatesArray
                |> Array.mapi(fun dex rates -> 
                                        TwoOrbitTypeOps.mutateTwoOrbitUnfolderStep floatPicker rates twoOrbitUf4.TwoOrbitUnfolderSteps[dex])

        TwoOrbitUf4.create twoOrbitTypeMutated twoOrbitUnfolderSteps



module UnfolderOps6 =
 
    // makeTwoOrbitUnfolder6
    let makeTwoOrbitUf6 (floatPicker:unit -> float) (uf6GenRates:Uf6GenRates) 
                : TwoOrbitUf6 =

        let seed6TwoOrbitType = 
                uf6GenRates.seedGenRatesUf6.PickMode floatPicker |> Seed6GenMode.toSeed6TwoOrbitType

        let twoOrbitUfSteps = uf6GenRates.opsGenRatesList
                                |> Array.mapi(
            fun dex rates -> TwoOrbitTypeOps.makeTwoOrbitUnfolderStep floatPicker (6 * (MathUtils.integerPower 2 dex)) rates)

        TwoOrbitUf6.create seed6TwoOrbitType twoOrbitUfSteps


    // mutateTwoOrbitUnfolder6
    let mutateTwoOrbitUf6
                (floatPicker:unit -> float)
                (uf6MutationRates:Uf6MutationRates) 
                (twoOrbitUf6:TwoOrbitUf6) : TwoOrbitUf6 =

        let seed6TwoOrbitTypeMutated = 
                uf6MutationRates.seed6TransitionRates.TransitionMode
                        floatPicker
                        (twoOrbitUf6.Seed6TwoOrbitType |> Seed6GenMode.fromSeed6TwoOrbitType)
                        |> Seed6GenMode.toSeed6TwoOrbitType

        let twoOrbitUnfolderSteps = 
                    uf6MutationRates.opsTransitionRates
                    |> Array.mapi(
                fun dex rates -> 
                    TwoOrbitTypeOps.mutateTwoOrbitUnfolderStep floatPicker rates twoOrbitUf6.TwoOrbitUnfolderSteps[dex])

        TwoOrbitUf6.create seed6TwoOrbitTypeMutated twoOrbitUnfolderSteps



