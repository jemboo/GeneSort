namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.Core

// ----------------------------------------------------------------------------
// Measure Types
// ----------------------------------------------------------------------------

[<Measure>] type sorterPoolSetHistoryCollectionId

// ----------------------------------------------------------------------------
// sorterPoolSetHistoryCollection Domain Type
// ----------------------------------------------------------------------------

type sorterPoolSetHistoryCollection = {
    CollectionId: Guid<sorterPoolSetHistoryCollectionId>
    Histories: sorterPoolSetHistory list
}

module sorterPoolSetHistoryCollection =

    let create 
            (collectionId: Guid<sorterPoolSetHistoryCollectionId>) 
            (histories: sorterPoolSetHistory list) 
            : sorterPoolSetHistoryCollection =
        {
            CollectionId = collectionId
            Histories = histories
        }