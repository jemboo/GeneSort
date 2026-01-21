namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable

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

    member this.MakeSortableBoolTests (sortingWidth: int<sortingWidth>) : sortableBoolTest =
        let sortableArrays =  SortableBoolArray.getAllSortableBoolArrays sortingWidth
        sortableBoolTest.create 
                ( %this.id |> UMX.tag<sorterTestId>) 
                sortingWidth
                sortableArrays

module MsasF = ()
 
 