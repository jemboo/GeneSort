namespace GeneSort.Core

module TwoOrbitTypeOps =

    ///makeTwoOrbitTypeGen
    let makeRandomTwoOrbitTypes 
            (floatPicker:unit -> float)
            (opsGenRates:OpsGenRates) 
            : TwoOrbitType seq =
        seq {
            while true do
                floatPicker |> opsGenRates.PickMode |> OpsGenMode.toTwoOrbitType
        }

    let mutateTwoOrbitTypes
            (floatPicker:unit -> float)
            (opsTransitionRates:OpsTransitionRates)
            (twoOrbitTypes:TwoOrbitType seq)
             : TwoOrbitType seq =

         twoOrbitTypes |> Seq.map( fun topt ->
                match topt with
                | TwoOrbitType.Ortho -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.Ortho) with
                    | OpsActionMode.Para -> TwoOrbitType.Para
                    | OpsActionMode.SelfRefl -> TwoOrbitType.SelfRefl
                    | _ -> TwoOrbitType.Ortho
 
                | TwoOrbitType.Para -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.Para) with
                    | OpsActionMode.Ortho -> TwoOrbitType.Ortho
                    | OpsActionMode.SelfRefl -> TwoOrbitType.SelfRefl
                    | _ -> TwoOrbitType.Para

                | TwoOrbitType.SelfRefl -> 
                    match (opsTransitionRates.PickMode floatPicker TwoOrbitType.SelfRefl) with
                    | OpsActionMode.Para -> TwoOrbitType.Para
                    | OpsActionMode.Ortho -> TwoOrbitType.Ortho
                    | _ -> TwoOrbitType.SelfRefl
                )


    // makeTwoOrbitUnfolderStep
    let makeRandomTwoOrbitUnfolderStep
                (floatPicker:unit -> float)
                (order:int) 
                (opsGenRates:OpsGenRates) 
                : TwoOrbitUfStep =
        TwoOrbitUfStep.create 
                (makeRandomTwoOrbitTypes floatPicker opsGenRates 
                    |> Seq.take (order / 2)
                    |> Seq.toList)
                order


    // mutateTwoOrbitUnfolderStep
    let mutateTwoOrbitUnfolderStep
                (floatPicker:unit -> float)
                (opsTransitionRates:OpsTransitionRates) 
                (twoOrbitUfStep:TwoOrbitUfStep) =
        TwoOrbitUfStep.create 
                (mutateTwoOrbitTypes floatPicker opsTransitionRates twoOrbitUfStep.twoOrbitTypes
                    |> Seq.take (twoOrbitUfStep.Order / 2)
                    |> Seq.toList)
                twoOrbitUfStep.Order
