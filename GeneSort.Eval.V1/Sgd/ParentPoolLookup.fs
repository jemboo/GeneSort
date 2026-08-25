namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Eval.V1


/// Custom domain type mapping a child pool ID to its parent pool ID
type parentPoolLookup = 
    private { 
        _lookupMap: Map<Guid<sorterPoolId>, Guid<sorterPoolId>> 
    }
    member this.LookupMap with get() = this._lookupMap

module ParentPoolLookup =

    let empty : parentPoolLookup = 
        { _lookupMap = Map.empty }

    let create (map: Map<Guid<sorterPoolId>, Guid<sorterPoolId>>) : parentPoolLookup = 
        { _lookupMap = map }

    let ofSeq (pairs: seq<Guid<sorterPoolId> * Guid<sorterPoolId>>) : parentPoolLookup = 
        pairs |> Map.ofSeq |> create

    /// Safe lookup to find the parent pool ID of a given child pool ID
    let tryFindParentId (childPoolId: Guid<sorterPoolId>) (lookup: parentPoolLookup) : Guid<sorterPoolId> option =
        Map.tryFind childPoolId lookup._lookupMap