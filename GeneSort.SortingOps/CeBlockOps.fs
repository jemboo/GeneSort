namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

module CeBlockOps = 


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortBy< ^a when ^a: comparison> 
                (ceBlock: ceBlock) 
                (ceUseCounts: ceUseCounts)
                (values: ^a[]) : ^a[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.getCe i
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                ceUseCounts.Increment i
        values

    let sortByBools
                (ceBlock: ceBlock) 
                (ceUseCounts: ceUseCounts)
                (values: bool[]) : bool[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.getCe i
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                ceUseCounts.Increment i
        values


    /// Evaluates a ceBlock against sortableTests, returning a new ceBlockEval with updated sorterTests and ceBlockUsage.
    /// Any duplicates in the resulting sorterTests are removed.
    let evalWithSorterTest
        (sortableTests: sortableTests)  
        (ceBlock: ceBlock) : ceBlockEval =

        let ceUseCounts = ceUseCounts.create ceBlock.Length 

        let newSorterTests =
            match sortableTests with
            | sortableTests.Ints sits -> 
                let newSortableIntArray = 
                    sits.SortableIntArrays |> Array.map(
                            fun sia ->
                            let valuesCopy = Array.copy sia.Values
                            let _ = sortBy ceBlock ceUseCounts valuesCopy |> ignore
                            sortableIntArray.Create(valuesCopy, sia.SortingWidth, sia.SymbolSetSize)
                        ) |> SortableIntArray.removeDuplicates

                sortableIntTests.create (Guid.NewGuid() |> UMX.tag<sortableTestsId>) newSortableIntArray   
                |> GeneSort.Sorter.Sortable.sortableTests.Ints
                 
            | sortableTests.Bools sbts ->
                let newSortableBoolArray = 
                    sbts.SortableBoolArrays |> Array.map(
                            fun sba ->
                            let valuesCopy = Array.copy sba.Values
                            let _ = sortByBools ceBlock ceUseCounts valuesCopy |> ignore
                            sortableBoolArray.Create(valuesCopy, sba.SortingWidth)
                        ) |> SortableBoolArray.removeDuplicates

                sortableBoolTests.create (Guid.NewGuid() |> UMX.tag<sortableTestsId>) newSortableBoolArray   
                |> GeneSort.Sorter.Sortable.sortableTests.Bools
            
        let ceBlockUsage = ceBlockWithUsage.create ceBlock (ceUseCounts.UseCounts)
        ceBlockEval.create ceBlockUsage newSorterTests

