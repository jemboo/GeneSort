namespace GeneSort.Model.Test.Sorter.Ce

open System
open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter.Ce
open GeneSort.Model.Sorter

type MsceRandGenTests() =

    // Helper to create a Model_Ce
    let createModelCe (id: Guid<sorterModelID>) (width: int<sortingWidth>) (ceCodes: int array) : Msce =
        Msce.create id width ceCodes

    // Helper function to create a mock random number generator
    let createMockRando (indices: int list) (floats: float list) =
        fun _ _ -> new MockRando(floats, indices) :> IRando
