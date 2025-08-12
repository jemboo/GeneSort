﻿namespace GeneSort.Core

open Combinatorics
open CollectionUtils
open TwoOrbitPairOps
open MathUtils


module PrototypeUnfolder =


    // randomRsOrbitPairTypeList
    let randomTwoOrbitTypeList (randy:IRando) (n: int) : TwoOrbitType list =
        if n < 0 then failwith "Length must be non-negative"
        let values = [| TwoOrbitType.Ortho; TwoOrbitType.Para; TwoOrbitType.SelfRefl |]
        List.init n (fun _ -> values.[randy.NextIndex(values.Length)])


    /// returns allTwoOrbitType lists of length n
    let allTwoOrbitTypeLists n =
        let allValues = [TwoOrbitType.Ortho; TwoOrbitType.Para; TwoOrbitType.SelfRefl]
        let inputLists = List.replicate n allValues
        cartesianProduct inputLists


    // creates TwoOrbitPairs from TwoOrbits by reflection
    let unfoldTwoOrbitPairsIntoTwoOrbitPairs 
            (types: TwoOrbitType list) 
            (twoOrbitPairs: TwoOrbitPair list) : TwoOrbitPair list =

        if types.Length <> (twoOrbitPairs.Length * 2) then
            invalidArg "types" "Length of types must be twice the length of pairs"
        if types.Length = 0 then
            []
        else
            let m = twoOrbitPairs.Length
            let order = twoOrbitPairs.[0].Order
            // Validate all pairs have the same order
            if twoOrbitPairs |> List.exists (fun p -> p.Order <> order) then
                invalidArg "pairs" "All TwoOrbitPairs must have the same order"
        
            // Generate two TwoOrbitPairs for each input pair
            let results =
                seq {
                    for i in 0 .. (m - 1) do
                        yield unfoldTwoOrbitIntoTwoOrbitPair twoOrbitPairs[i].First order types[2 * i]
                        if (twoOrbitPairs[i].Second.IsSome) then
                            yield unfoldTwoOrbitIntoTwoOrbitPair twoOrbitPairs[i].Second.Value order types[2 * i + 1]
                } |> Seq.toList

            if results.Length <> 2 * m then
                failwith "Unexpected number of TwoOrbitPairs generated"
            results




    // twoOrbitPairsForOrder4
    let twoOrbitPairsForOrder4 (twoOrbitType:TwoOrbitType) : TwoOrbitPair =
        match twoOrbitType with
        | TwoOrbitType.Ortho -> TwoOrbitPair.create 4 (TwoOrbit.create [0; 2]) (TwoOrbit.create [1; 3] |> Some)
        | TwoOrbitType.Para -> TwoOrbitPair.create 4 (TwoOrbit.create [0; 1]) (TwoOrbit.create [2; 3] |> Some)
        | TwoOrbitType.SelfRefl ->  TwoOrbitPair.create 4 (TwoOrbit.create [0; 3]) (TwoOrbit.create [1; 2] |> Some)


    // makeTwoCycleFromTwoOrbitTypes
    let makePerm_SiFromTwoOrbitTypes
            (seedTwoOrbitPairs : TwoOrbitPair list option)
            (twoOrbitTypes: TwoOrbitType list) : Perm_Si =
        if twoOrbitTypes.Length < 1 then
            failwith "twoOrbitTypes list must have an element"

        let _makeTwoCycleFromTwoOrbitTypes
                (seedTwoOrbitPairs : TwoOrbitPair list)
                (twoOrbitTypes: TwoOrbitType list)  : Perm_Si =
            if seedTwoOrbitPairs.Length < 1 then
                failwith "seedRsOrbitPair list must have an element"
            if twoOrbitTypes.Length < 1 then
                failwith "twoOrbitTypes list must have an element"

            let rsOrbitPairTypeChunks = 
                    chunkByPowersOfTwo 
                        (seedTwoOrbitPairs.Length * 2)
                        twoOrbitTypes 
                        |> Seq.map Seq.toList |> Seq.toList
            let mutable workingList = seedTwoOrbitPairs
            for chunk in rsOrbitPairTypeChunks do
                workingList <- unfoldTwoOrbitPairsIntoTwoOrbitPairs chunk workingList 

            Perm_Si.fromTwoOrbitPair (workingList |> List.toArray)

        match seedTwoOrbitPairs with
        | Some orbitPairs -> 
                _makeTwoCycleFromTwoOrbitTypes orbitPairs twoOrbitTypes
        | None -> 
                let h::t = twoOrbitTypes
                let seedPrs = [twoOrbitPairsForOrder4 h]
                _makeTwoCycleFromTwoOrbitTypes seedPrs t



    // makeAllPerm_SisOfOrder
    let makeAllPerm_SisOfOrder (order:int) 
            : Perm_Si seq =
        if (not (isAPowerOfTwo order)) then
            failwith "order must be a power of two"
        if(order < 4) then
            failwith "order must be at least 4"

        let rec _seqLen v c =
            if v = 4 then c + 1
            else _seqLen (v/2) (c + v/4)

        let seqLength = _seqLen order 0
        (allTwoOrbitTypeLists seqLength) 
        |> Seq.map(makePerm_SiFromTwoOrbitTypes None)


    // makeRandomPerm_Sis
    let makeRandomPerm_Sis (randy:IRando) (order:int) 
            : Perm_Si seq =
    
        // getTwoOrbitTypeLengthForOrder
        let getTwoOrbitTypeLengthForOrder (order:int) =
            if (not (isAPowerOfTwo order)) then
                failwith "order must be a power of two"
            if(order < 4) then
                failwith "order must be at least 4"
            let rec _seqLen v c =
                if v = 4 then c + 1
                else _seqLen (v/2) (c + v/4)
        
            _seqLen order 0

        let seqLength = getTwoOrbitTypeLengthForOrder order
        seq {
                while true do
                    let rsTypes = randomTwoOrbitTypeList randy seqLength
                    yield makePerm_SiFromTwoOrbitTypes None rsTypes
            }

                    




