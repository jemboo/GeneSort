namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

// MsasMi = all int array merge cases
[<Struct; CustomEquality; NoComparison>]
type MsasMi = 
    private 
        { id: Guid<sorterTestModelID>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            : MsasMi =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        else
            let id = 
                [
                    "MsasMi" :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sorterTestModelID>
            { id = id; sortingWidth = sortingWidth; }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasMi as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<MsasMi> with
        member this.Equals(other) =  this.sortingWidth = other.sortingWidth

    member this.MakeSortableTests (sortingWidth: int<sortingWidth>) : sortableTests =
        sortableIntTests.create 
                ( %this.id |> UMX.tag<sortableTestsId>) 
                sortingWidth
                (SortableIntArray.getIntArrayMerge2Cases sortingWidth)
        |> sortableTests.Ints


module MsasMi = ()
 
 