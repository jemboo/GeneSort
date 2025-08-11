namespace GeneSort.Core

open System
open FSharp.UMX
open Combinatorics


module Perm_RsOps = 

    // Create a random Perm_Rs
    let makeRandomPerm_Rs
            (indexPicker: int -> int) 
            (floatPicker: unit -> float)
            (opsGenRates: OpsGenRates) 
            (order: int)  : Perm_Rs =
        if(order % 2 <> 0) then
            failwith "Perm_Rs order must be even"
            
        let mutationModePicker () = 
                opsGenRates.PickMode floatPicker

        let rsPairTracker = Array.init (order / 2) (fun dex -> ((dex, reflect order dex), true))
        let _availableFlags () = rsPairTracker |> Seq.filter (fun (_, f) -> f)

        let twoOrbits =
            seq {
                while (_availableFlags () |> Seq.length > 0) do
                    if (_availableFlags () |> Seq.length < 2) then
                        // Since only one pair is left, just yield it
                        let lastOne = _availableFlags () |> Seq.head |> fst
                        rsPairTracker.[fst lastOne] <- (lastOne, false)
                        yield lastOne
                    else
                        let (dex1, dex2) = pickAndOrderTwoDistinct indexPicker (_availableFlags () |> Seq.length)
                        let pair1 = rsPairTracker.[dex1] |> fst
                        let pair2 = rsPairTracker.[dex2] |> fst 
                        rsPairTracker.[dex1] <- (pair1, false)
                        rsPairTracker.[dex2] <- (pair2, false)
                        let mode = mutationModePicker ()
                        match mode with
                        | OpsGenMode.SelfRefl ->
                            // return original self-symmetric pairs
                            yield pair1
                            yield pair2
                        | OpsGenMode.Ortho ->
                            // reconfigure pairs as Ortho pairs
                            yield (fst pair1, fst pair2)
                            yield (snd pair1, snd pair2)
                        | OpsGenMode.Para ->
                            // reconfigure pairs as Para pairs
                            yield (fst pair1, snd pair2)
                            yield (fst pair2, snd pair1)
            } |> Seq.toList

        
        let perm_Si = twoOrbits |> Perm_Si.fromTranspositions order

        Perm_Rs.create perm_Si.Array




    /// Analyzes two distinct points in a Perm_Rs permutation to determine their symmetry properties.
    /// <param name="indexPicker">Function to select a random index from 0 to n-1.</param>
    /// <param name="permRs">The Perm_Rs permutation to analyze, which must be self-inverse.</param>
    /// <returns>A TwoOrbitPair </returns>
    /// <exception cref="ArgumentException">Thrown if permRs.Order is less than 4 or not even, or if PermPair constraints are violated.</exception>
    let findRsPoints (indexPicker: int -> int) (permRs: Perm_Rs) : TwoOrbitPair =
        // Validate input
        let order = %permRs.Order
        let permArray = permRs.Array

        let _randomlyFindOtherReflectionSymmetricTwoOrbits (indexToExclude:int) =
            let rsTwoOrbs = permRs.Perm_Si 
                           |> Perm_Si.getTwoOrbits 
                           |> Array.filter(fun tob -> 
                            (tob |> TwoOrbit.isReflectionSymmetric %permRs.Order) && (tob.First <> indexToExclude))
            if rsTwoOrbs.Length = 0 then
               None
            else
               Some (rsTwoOrbs[indexPicker (rsTwoOrbs.Length)])

        // Select two distinct indices in ascending order
        let first, second = pickAndOrderTwoDistinct indexPicker (order / 2)
        
        // Compute reflection indices
        let firstReflect = reflect order first
        let secondReflect = reflect order second
        
        // Get mapped values from the permutation
        let firstMap = permArray.[first]
        let secondMap = permArray.[second]
        
        if firstReflect = firstMap then
            if secondReflect = secondMap then
                // Both points are self-symmetric
                TwoOrbitPair.create order (TwoOrbit.create [ first; firstMap ]) (TwoOrbit.create [ second; secondMap] |> Some) 
            else
                // First point is self-symmetric, second is not
                let otherRsTwoOrbit = _randomlyFindOtherReflectionSymmetricTwoOrbits first
                match otherRsTwoOrbit with
                | Some orb -> 
                    TwoOrbitPair.create order (TwoOrbit.create [ first; firstMap ]) (orb |> Some) 
                | None ->
                    let secondReflectMap = permArray.[secondReflect]
                    TwoOrbitPair.create order (TwoOrbit.create [ second; secondMap ]) (TwoOrbit.create [ secondReflect; secondReflectMap] |> Some) 
        else
            // Neither point is self-symmetric, use first point's reflection
            let firstReflectMap = permArray.[firstReflect]
            TwoOrbitPair.create order (TwoOrbit.create [ first; firstMap ]) (TwoOrbit.create [ firstReflect; firstReflectMap] |> Some) 



    // Mutates a Perm_Rs based on opsActionMode
    let mutatePerm_Rs
            (indexPicker: int -> int) 
            (opsActionMode:OpsActionMode) 
            (permRs: Perm_Rs) : Perm_Rs =

        if (opsActionMode = OpsActionMode.NoAction) then
            // for NoAction mode, return the permutation as is
            permRs
        else
            let newArray = Array.copy permRs.Array
            // Find two points in the permutation to mutate
            let twoOrbitPair = findRsPoints indexPicker permRs

            let twoOrbitTypeOpt = twoOrbitPair |> TwoOrbitPairOps.getTwoOrbitPairTypeOption
            let twoOrbitPairTypeFound = 
                match twoOrbitTypeOpt with
                | Some t -> t
                | None -> failwith "TwoOrbitPair must have a valid TwoOrbitPairType"
            let secondPair = 
                match twoOrbitPair.SecondOrbit with
                | Some sto -> sto
                | None -> failwith "TwoOrbitPair must have a SecondOrbit"

            let firstL = twoOrbitPair.FirstOrbit.First
            let firstH = twoOrbitPair.FirstOrbit.Second
            let secondL = secondPair.First
            let secondH = secondPair.Second

            match opsActionMode with
            | OpsActionMode.Ortho -> 
                match twoOrbitPairTypeFound with
                | TwoOrbitPairType.Ortho -> ()

                | _ -> 
                    newArray.[firstL] <- secondL
                    newArray.[secondL] <- firstL 
                    newArray.[firstH] <- secondH
                    newArray.[secondH] <- firstH 

            | OpsActionMode.Para -> 
                match twoOrbitPairTypeFound with
                | TwoOrbitPairType.Ortho -> 
                    newArray.[firstL] <- secondL
                    newArray.[secondL] <- firstL 
                    newArray.[firstH] <- secondH
                    newArray.[secondH] <- firstH

                | TwoOrbitPairType.Para -> ()

                | TwoOrbitPairType.SelfRefl -> 
                    newArray.[firstL] <- secondH
                    newArray.[secondH] <- firstL 
                    newArray.[firstH] <- secondL
                    newArray.[secondL] <- firstH 

            | OpsActionMode.SelfRefl -> 
                match twoOrbitPairTypeFound with
                | TwoOrbitPairType.SelfRefl -> ()
                | _ -> 
                    newArray.[firstL] <- secondH
                    newArray.[secondH] <- firstL 
                    newArray.[secondL] <- firstH
                    newArray.[firstH] <- secondL 
            | OpsActionMode.NoAction -> failwith "NoAction mode should not reach here"

            newArray |> Perm_Rs.createUnsafe

