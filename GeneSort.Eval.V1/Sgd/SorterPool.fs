namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1

// --- Types ---

type sorterPoolMember =
    private {
        _sorterPoolMemberId:  Guid<sorterPoolMemberId>
        _sorterModel: sorterModel
        _sorterMutationIndex: int<sorterMutationIndex>
    }

type SorterPool =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolMembers: Map<Guid<sorterPoolMemberId>, sorterPoolMember>
    }








module SorterPool = ()
    
