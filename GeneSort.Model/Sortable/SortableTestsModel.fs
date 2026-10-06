namespace GeneSort.Model.Sortable

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sortable


type sortableTestsModel =
     | MsasF of msasF     // MsasF = a full bool test set for a given sorting width
     | MsasO of msasO     // MsasO = generated from a seed permutation; for bool models, it's expanded from the integer permutations
     | MsasMi of msasM    // All (sorting width)/2 merge test cases


module SortableTestsModel =

    let getSortingWidth (sortableTestsModel: sortableTestsModel): int<sortingWidth> =
        match sortableTestsModel with
        | MsasF msasF -> msasF.sortingWidth
        | MsasO msasO -> %msasO.SeedPermutation.Order |> UMX.tag<sortingWidth>
        | MsasMi msasMi -> msasMi.sortingWidth


    let makeSortableTests 
            (sortableTestsId: Guid<sortableTestsId>)
            (sortableTestsModel: sortableTestsModel) 
            (sortableDataFormat: sortableDataFormat) : sortableTests =

        match sortableTestsModel with

        | MsasF msasF -> 
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                    (msasF.MakeSortableBoolTest sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Bools
                | sortableDataFormat.IntArray ->
                    (msasF.MakeSortableIntTest sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Ints
                | sortableDataFormat.BitVector256 ->
                    failwith "BitVector256 SortableArrayType not supported"
                | sortableDataFormat.BitVector512 ->
                    (msasF.MakeSortableBitv512Test sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Bitv512
                | sortableDataFormat.Int8Vector256 ->
                    (msasF.MakeSortableUint8v256Test sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Uint8v256
                | sortableDataFormat.Int8Vector512  -> 
                    (msasF.MakeSortableUint8v512Test sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Uint8v512
                | sortableDataFormat.PackedIntArray ->
                    failwith "PackedIntArray SortableArrayType not supported"

        | MsasO msasO ->
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                     (msasO.MakeSortableBoolTest sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Bools
                | sortableDataFormat.IntArray ->
                     (msasO.MakeSortableIntTest sortableTestsId (getSortingWidth sortableTestsModel)) |> sortableTests.Ints
                | sortableDataFormat.Int8Vector256 ->
                     failwith "Int8Vector256 SortableArrayType not supported"
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasO"

        | MsasMi msasMi ->
                match sortableDataFormat with
                | sortableDataFormat.BoolArray ->        
                    (msasMi.MakeSortableBoolTest sortableTestsId) |> sortableTests.Bools
                | sortableDataFormat.IntArray ->
                    (msasMi.MakeSortableIntTest sortableTestsId) |> sortableTests.Ints
                | sortableDataFormat.Int8Vector256 -> 
                    (msasMi.MakeSortableUint8v256Test sortableTestsId) |> sortableTests.Uint8v256
                | sortableDataFormat.Int8Vector512 -> 
                    (msasMi.MakeSortableUint8v512Test sortableTestsId) |> sortableTests.Uint8v512
                | sortableDataFormat.BitVector512 ->
                    (msasMi.MakeSortableBitv512Test sortableTestsId) |> sortableTests.Bitv512
                | _ ->  
                    failwith "Unsupported SortableArrayType for MsasMi"
                    