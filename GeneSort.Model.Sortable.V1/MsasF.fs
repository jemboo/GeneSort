namespace GeneSort.Model.Sortable.V1

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sortable.SortableIntArray

// MsasF = a full bool test set for a given sorting width
[<Struct; CustomEquality; NoComparison>]
type msasF = 
    private 
        { id: Guid<sorterTestModelID>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            : msasF =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        else
            let id = 
                [
                    box "msasF"
                    box (sortingWidth |> UMX.untag)
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { id = id; sortingWidth = sortingWidth; }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? msasF as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<msasF> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSortableBoolTest 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortingWidth: int<sortingWidth>) : sortableBinaryTests =
        let sortableArrays =  SortableBoolArray.getAllSortableBoolArrays sortingWidth
                              |> Seq.toArray
        sortableBinaryTests.create 
                sorterTestId
                sortingWidth
                sortableArrays


    member this.MakeSortableIntTest 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortingWidth: int<sortingWidth>) : sortableIntTests =
        let sortableArrays =  BinaryIntArrays.getAllBinaryIntArraysForSortingWidth sortingWidth
        sortableIntTests.create
                sorterTestId
                sortingWidth
                sortableArrays


    member this.MakeSortableBitv512Test 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortingWidth: int<sortingWidth>) : sortableBitv512Tests =
        let grayBlocks = Sortable.GrayVectorGenerator.getAllSortBlockBitv512ForSortingWidth sortingWidth |> Seq.toArray
        sortableBitv512Tests.create
                sorterTestId
                sortingWidth
                grayBlocks


    member this.MakeSortableUint8v256Test 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortingWidth: int<sortingWidth>) : sortableUint8v256Tests =
        let sortableArrays =  BinaryIntArrays.getAllBinaryIntArraysForSortingWidth sortingWidth
        SortableUint8v256Tests.fromIntArrays
                sorterTestId
                sortingWidth
                sortableArrays


    member this.MakeSortableUint8v512Test 
                    (sorterTestId: Guid<sortableTestsId>)
                    (sortingWidth: int<sortingWidth>) : sortableUint8v512Tests =
        let sortableArrays =  BinaryIntArrays.getAllBinaryIntArraysForSortingWidth sortingWidth
        SortableUint8v512Tests.fromIntArrays
                sorterTestId
                sortingWidth
                sortableArrays


module MsasF = ()
 
 