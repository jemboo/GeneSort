namespace GeneSort.Core


module TwoOrbitPairOps =
    /// Gets the TwoOrbitType of the TwoOrbitPair
    /// returns the TwoOrbitType of the first TwoOrbit if the second is None,
    /// or checks the TwoOrbitType of both if the second is present.
    /// returns None if the TwoOrbits are not of the same type.
    /// <param name="twoOrbitPair">The TwoOrbitPair to check.</param>
    let getTwoOrbitTypeOption (twoOrbitPair: TwoOrbitPair) : TwoOrbitType option =

        match twoOrbitPair.Second with
        | None ->
            Some (TwoOrbit.getTwoOrbitType twoOrbitPair.Order twoOrbitPair.FirstOrbit)
        | Some second ->
          match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order twoOrbitPair.FirstOrbit) with
            | TwoOrbitType.Ortho ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | TwoOrbitType.Ortho -> Some TwoOrbitType.Ortho 
                | _ -> None
            | TwoOrbitType.Para ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | TwoOrbitType.Para -> Some TwoOrbitType.Para 
                | _ -> None
            | TwoOrbitType.SelfRefl ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | TwoOrbitType.SelfRefl -> Some TwoOrbitType.SelfRefl 
                | _ -> None


    let getTwoOrbitType (twoOrbitPair:TwoOrbitPair) : TwoOrbitType =
        if twoOrbitPair.Second.IsNone then
            TwoOrbitType.SelfRefl
        else
            let orbit1 = twoOrbitPair.First
            if TwoOrbit.isReflectionSymmetric twoOrbitPair.Order orbit1  then
                TwoOrbitType.SelfRefl
            else
                if ((snd orbit1.IndicesTuple) < twoOrbitPair.Order / 2) then
                    TwoOrbitType.Ortho
                else
                    TwoOrbitType.Para


    let fromTwoOrbits (order:int) (twoOrbits:seq<TwoOrbit>) : seq<TwoOrbitPair> =
        twoOrbits |> CollectionUtils.pairWithNext 
                  |> Seq.map(fun (to1, to2) -> TwoOrbitPair.create order to1 to2)

    
    let makeTwoOrbits (twoOrbitPairs:seq<TwoOrbitPair>) : seq<TwoOrbit> =
        seq {
            for top in twoOrbitPairs do
                yield top.FirstOrbit
                if (top.SecondOrbit.IsSome) then
                    yield top.SecondOrbit.Value
        }


    // creates a TwoOrbitPair from a TwoOrbit by reflection
    let unfoldTwoOrbitIntoTwoOrbitPair
            (twoOrbit:TwoOrbit) 
            (order:int) 
            (twoOrbitType:TwoOrbitType) : TwoOrbitPair =

        let (low, high) = twoOrbit.IndicesTuple
        let (highR, lowR) = (TwoOrbit.getReflection (order*2) twoOrbit).IndicesTuple
        let (twoOrbitA, twoOrbitB) = 
            match twoOrbitType with
            | TwoOrbitType.Ortho -> (TwoOrbit.create [low; high], TwoOrbit.create [lowR; highR])
            | TwoOrbitType.Para -> (TwoOrbit.create [low; highR], TwoOrbit.create [high; lowR])
            | TwoOrbitType.SelfRefl -> (TwoOrbit.create [low; lowR], TwoOrbit.create [high; highR])

        TwoOrbitPair.create (order*2) twoOrbitA (Some twoOrbitB)
