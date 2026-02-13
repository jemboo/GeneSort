namespace GeneSort.Core

module TwoOrbitTypeOps =

    ///makeTwoOrbitTypeGen
    let makeRandomTwoOrbitPairTypes 
            (floatPicker:unit -> float)
            (opsGenRates:opsGenRates) 
            : TwoOrbitPairType seq =
        seq {
            while true do
                floatPicker |> opsGenRates.PickMode |> OpsGenMode.toTwoOrbitPairType
        }

    let mutateTwoOrbitPairTypes
            (floatPicker:unit -> float)
            (opsTransitionRates:opsTransitionRates)
            (twoOrbitTypes:TwoOrbitPairType seq)
             : TwoOrbitPairType seq =

         twoOrbitTypes |> Seq.map( fun topt ->
                match topt with
                | TwoOrbitPairType.Ortho -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.Ortho) with
                    | opsActionMode.Para -> TwoOrbitPairType.Para
                    | opsActionMode.SelfRefl -> TwoOrbitPairType.SelfRefl
                    | _ -> TwoOrbitPairType.Ortho
 
                | TwoOrbitPairType.Para -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.Para) with
                    | opsActionMode.Ortho -> TwoOrbitPairType.Ortho
                    | opsActionMode.SelfRefl -> TwoOrbitPairType.SelfRefl
                    | _ -> TwoOrbitPairType.Para

                | TwoOrbitPairType.SelfRefl -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.SelfRefl) with
                    | opsActionMode.Para -> TwoOrbitPairType.Para
                    | opsActionMode.Ortho -> TwoOrbitPairType.Ortho
                    | _ -> TwoOrbitPairType.SelfRefl
                )


    // makeTwoOrbitUnfolderStep
    let makeRandomTwoOrbitUnfolderStep
                (floatPicker:unit -> float)
                (order:int) 
                (opsGenRates:opsGenRates) 
                : TwoOrbitUfStep =
        TwoOrbitUfStep.create 
                (makeRandomTwoOrbitPairTypes floatPicker opsGenRates 
                    |> Seq.take (order / 2)
                    |> Seq.toArray)
                order


    // mutateTwoOrbitUnfolderStep
    let mutateTwoOrbitUnfolderStep
                (floatPicker:unit -> float)
                (opsTransitionRates:opsTransitionRates) 
                (twoOrbitUfStep:TwoOrbitUfStep) =
        TwoOrbitUfStep.create 
                (mutateTwoOrbitPairTypes floatPicker opsTransitionRates twoOrbitUfStep.TwoOrbitPairTypes
                    |> Seq.take (twoOrbitUfStep.Order / 2)
                    |> Seq.toArray)
                twoOrbitUfStep.Order
