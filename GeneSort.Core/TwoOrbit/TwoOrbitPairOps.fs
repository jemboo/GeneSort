namespace GeneSort.Core


module TwoOrbitPairOps =
    /// Gets the TwoOrbitType of the TwoOrbitPair
    /// returns the TwoOrbitType of the first TwoOrbit if the second is None,
    /// or checks the TwoOrbitType of both if the second is present.
    /// returns None if the TwoOrbits are not of the same type.
    /// <param name="twoOrbitPair">The TwoOrbitPair to check.</param>
    let getTwoOrbitPairTypeOption (twoOrbitPair: TwoOrbitPair) : twoOrbitPairType option =

        match twoOrbitPair.Second with
        | None -> 
               match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order twoOrbitPair.FirstOrbit) with
               | twoOrbitType.Ortho -> Some twoOrbitPairType.Ortho
               | twoOrbitType.Para -> Some twoOrbitPairType.Para
               | twoOrbitType.SelfRefl -> Some twoOrbitPairType.SelfRefl
        | Some second ->
          match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order twoOrbitPair.FirstOrbit) with
            | twoOrbitType.Ortho ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | twoOrbitType.Ortho -> Some twoOrbitPairType.Ortho 
                | _ -> None
            | twoOrbitType.Para ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | twoOrbitType.Para -> Some twoOrbitPairType.Para 
                | _ -> None
            | twoOrbitType.SelfRefl ->
                match (TwoOrbit.getTwoOrbitType twoOrbitPair.Order second) with
                | twoOrbitType.SelfRefl -> Some twoOrbitPairType.SelfRefl 
                | _ -> None


    let getTwoOrbitPairType (twoOrbitPair:TwoOrbitPair) : twoOrbitPairType =
        if twoOrbitPair.Second.IsNone then
            twoOrbitPairType.SelfRefl
        else
            let orbit1 = twoOrbitPair.First
            if TwoOrbit.isReflectionSymmetric twoOrbitPair.Order orbit1  then
                twoOrbitPairType.SelfRefl
            else
                if ((snd orbit1.IndicesTuple) < twoOrbitPair.Order / 2) then
                    twoOrbitPairType.Ortho
                else
                    twoOrbitPairType.Para


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
            (twoOrbitPairType:twoOrbitPairType) : TwoOrbitPair =

        let (low, high) = twoOrbit.IndicesTuple
        let (highR, lowR) = (TwoOrbit.getReflection (order*2) twoOrbit).IndicesTuple
        let (twoOrbitA, twoOrbitB) = 
            match twoOrbitPairType with
            | twoOrbitPairType.Ortho -> (TwoOrbit.create [low; high], TwoOrbit.create [lowR; highR])
            | twoOrbitPairType.Para -> (TwoOrbit.create [low; highR], TwoOrbit.create [high; lowR])
            | twoOrbitPairType.SelfRefl -> (TwoOrbit.create [low; lowR], TwoOrbit.create [high; highR])

        TwoOrbitPair.create (order*2) twoOrbitA (Some twoOrbitB)
