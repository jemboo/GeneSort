namespace GeneSort.Model.Sortable

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
                    "MsasF" :> obj
                    sortingWidth :> obj
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

    member this.MakeSortableBoolTest (sortingWidth: int<sortingWidth>) : sortableBinaryTest =
        let sortableArrays =  SortableBoolArray.getAllSortableBoolArrays sortingWidth
        sortableBinaryTest.create 
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                sortableArrays

    member this.MakeSortableIntTest (sortingWidth: int<sortingWidth>) : sortableIntTest =
        let sortableArrays =  BinaryArrayUtils.getAllSortableBinaryArrays sortingWidth
        sortableIntTest.create
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                sortableArrays

    member this.MakeSortableBitv512Test (sortingWidth: int<sortingWidth>) : sortableBitv512Test =
        let grayBlocks = Sortable.GrayVectorGenerator.getAllSortBlockBitv512ForSortingWidth sortingWidth |> Seq.toArray
        sortableBitv512Test.create
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                grayBlocks

    member this.MakeSortableUint8v256Test (sortingWidth: int<sortingWidth>) : sortableUint8v256Test =
        let sortableArrays =  BinaryArrayUtils.getAllSortableBinaryArrays sortingWidth
        SortableUint8v256Test.fromIntArrays
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                sortableArrays

    member this.MakeSortableUint8v512Test (sortingWidth: int<sortingWidth>) : sortableUint8v512Test =
        let sortableArrays =  BinaryArrayUtils.getAllSortableBinaryArrays sortingWidth
        SortableUint8v512Test.fromIntArrays
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                sortableArrays



module MsasF = ()
 
 