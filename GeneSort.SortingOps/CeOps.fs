namespace GeneSort.SortingOps

open System
open FSharp.UMX
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

module CeOps = 


   // mutates in placeby a sequence of ces, and returns the resulting sortable (values[]),
   // records the number of uses of each ce in useCounter, starting at useCounterOffset
    let inline sortBy< ^a when ^a: comparison> 
                (ceBlock: ceBlock) 
                (ceBlockUsage: ceBlockUsage)
                (values: ^a[]) : ^a[] =

        for i = 0 to %ceBlock.Length - 1 do
            let ce = ceBlock.getCe i
            if values.[ce.Low] > values.[ce.Hi] then
                let temp = values.[ce.Low]
                values.[ce.Low] <- values.[ce.Hi]
                values.[ce.Hi] <- temp
                ceBlockUsage.Increment i
        values


    /// Evaluates a ceBlock against sorterTests, returning a new ceBlockEval with updated sorterTests and ceBlockUsage.
    /// Any duplicates in the resulting sorterTests are removed.
    let evalWithSorterTests
        (sorterTests: sorterTests)  
        (ceBlock: ceBlock) : ceBlockEval =

        let ceBlockUsage = ceBlockUsage.create ceBlock.Length 

        let newSorterTests =
            match sorterTests with
            | sorterTests.Ints sits -> 
                let newSortableIntArray = 
                    sits.SortableIntArrays |> Array.map(
                            fun sia ->
                            let valuesCopy = Array.copy sia.Values
                            let _ = sortBy ceBlock ceBlockUsage valuesCopy |> ignore
                            sortableIntArray.Create(valuesCopy, sia.SortingWidth, sia.SymbolSetSize)
                        ) |> SortableIntArray.removeDuplicates

                sorterIntTests.create (Guid.NewGuid() |> UMX.tag<sorterTestIsd>) newSortableIntArray   
                |> GeneSort.Sorter.Sortable.sorterTests.Ints
                 
            | sorterTests.Bools sbts ->
                let newSortableBoolArray = 
                    sbts.SortableBoolArrays |> Array.map(
                            fun sba ->
                            let valuesCopy = Array.copy sba.Values
                            let _ = sortBy ceBlock ceBlockUsage valuesCopy |> ignore
                            sortableBoolArray.Create(valuesCopy, sba.SortingWidth)
                        ) |> SortableBoolArray.removeDuplicates

                sorterBoolTests.create (Guid.NewGuid() |> UMX.tag<sorterTestIsd>) newSortableBoolArray   
                |> GeneSort.Sorter.Sortable.sorterTests.Bools
            

        ceBlockEval.create(ceBlock, newSorterTests, ceBlockUsage)

