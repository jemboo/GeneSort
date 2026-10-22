namespace GeneSort.Model.Test.Sorter.Ce

open System
open FSharp.UMX
open Xunit
open GeneSort.Core
open GeneSort.Core.Rando
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple
open GeneSort.Model.Sorting.V1.Simple.Ce

type MsceRandGenTests() =

    // Helper to create a Model_Ce
    let createModelCe (id: Guid<sorterModelId>) (width: int<sortingWidth>) (ceCodes: int array) : msce =
        msce.create id width ceCodes

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando
