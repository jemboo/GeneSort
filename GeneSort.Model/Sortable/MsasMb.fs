namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

// MsasMb = merge bool array test cases
[<Struct; CustomEquality; NoComparison>]
type MsasMb = 
    private 
        { id: Guid<sorterTestModelID>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            : MsasMb =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        else
            let id = 
                [
                    "MsasMb" :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { id = id; sortingWidth = sortingWidth; }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasMb as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<MsasMb> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSortableTests (sortingWidth: int<sortingWidth>) : sortableTests =
        let sortableArrays =  SortableBoolArray.getMergeSortTestCases sortingWidth
        sortableBoolTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                sortingWidth
                sortableArrays |> sortableTests.Bools


module MsasMb = ()
 
 