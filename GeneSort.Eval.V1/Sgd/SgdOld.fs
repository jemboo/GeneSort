namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1

// --- Types ---

type sorterMutationSource =
    private {
        _sorterModelMutatorId: Guid<sorterModelMutatorId>
        _sorterModelId: Guid<sorterModelId>
        _mutationIndex: int<sorterMutationIndex>
    }
    member this.SorterModelMutatorId = this._sorterModelMutatorId
    member this.SorterModelId = this._sorterModelId
    member this.MutationIndex = this._mutationIndex

    static member Create(sorterModelMutatorId, parentSorterModelId, parentMutationIndex) =
        { 
            _sorterModelMutatorId = sorterModelMutatorId
            _sorterModelId = parentSorterModelId
            _mutationIndex = parentMutationIndex 
        }

type sorterModelSgd = 
    private {
        _sorterModel: sorterModel
        _mutationIndex: int<sorterMutationIndex>
        _sorterMutationSource: sorterMutationSource option
    }
    member this.SorterModel = this._sorterModel
    member this.MutationIndex = this._mutationIndex
    member this.SorterMutationSource = this._sorterMutationSource

    static member Create(sorterModel, mutationIndex, sorterMutationSource) =
        { 
            _sorterModel = sorterModel
            _mutationIndex = mutationIndex
            _sorterMutationSource = sorterMutationSource 
        }


type sorterModelSgd2 =
    private {
        _sorterModelSgd: sorterModelSgd
        _sorterEval: sorterEval
    }
    member this.SorterModelSgd = this._sorterModelSgd
    member this.SorterEval = this._sorterEval

    static member Create(sorterModelSgd, sorterEval) =
        { 
            _sorterModelSgd = sorterModelSgd
            _sorterEval = sorterEval 
        }

[<CustomEquality; CustomComparison>]
type SorterPoolOld =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterModelSgd2s: sorterModelSgd2 array
    }
    member this.SorterPoolId = this._sorterPoolId
    member this.SorterModelSgd2s = this._sorterModelSgd2s

    static member Create(sorterPoolId, ?sorterModelSgd2s) =
        { 
            _sorterPoolId = sorterPoolId
            _sorterModelSgd2s = defaultArg sorterModelSgd2s [||] 
        }

    override this.Equals(obj) =
        match obj with
        | :? SorterPoolOld as other -> this._sorterPoolId = other._sorterPoolId
        | _ -> false
        
    override this.GetHashCode() = hash this._sorterPoolId
    
    interface IComparable with
        member this.CompareTo(obj) =
            match obj with
            | :? SorterPoolOld as other -> compare this._sorterPoolId other._sorterPoolId
            | _ -> invalidArg "obj" "Cannot compare SorterPool to a different type"
            


type SorterPoolSetOld =
    private {
        // Optimized: Changed from Map<sorterPool, sorterPool> to Map keyed by Guid
        _sorterPools: Map<Guid<sorterPoolId>, SorterPoolOld>
        _generationNumber: int<generationNumber>
    }
    member this.SorterPools = this._sorterPools
    member this.GenerationNumber = this._generationNumber

    static member Create(sorterPools: seq<SorterPoolOld>, generationNumber) =
        let poolMap = 
            sorterPools 
            |> Seq.map (fun p -> p.SorterPoolId, p) 
            |> Map.ofSeq
        { 
            _sorterPools = poolMap
            _generationNumber = generationNumber 
        }


// --- Active Modules ---

module Sgd = 
    
    /// Helper to easily extract and find a pool inside a SorterPoolSet
    let tryFindPool (poolId: Guid<sorterPoolId>) (poolSet: SorterPoolSetOld) =
        Map.tryFind poolId poolSet.SorterPools

    /// Adds or updates a SorterPool inside the SorterPoolSet
    let upsertPool (pool: SorterPoolOld) (poolSet: SorterPoolSetOld) =
        let updatedMap = Map.add pool.SorterPoolId pool poolSet.SorterPools
        SorterPoolSetOld.Create(Map.values updatedMap, poolSet.GenerationNumber)