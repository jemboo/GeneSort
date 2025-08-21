namespace GeneSort.Model.Sortable

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorter
open GeneSort.Model.Sorter


[<Struct; CustomEquality; NoComparison>]
type MsasF = 
    private 
        { id: Guid<sortableSetModelID>
          sortingWidth: int<sortingWidth> }

    static member create 
            (sortingWidth: int<sortingWidth>)
            : MsasF =
        if %sortingWidth < 2 then
            failwith "SortingWidth must be at least 2"
        else
            let id = 
                [
                    "MsasF" :> obj
                    sortingWidth :> obj
                ] |> GuidUtils.guidFromObjs |> UMX.tag<sortableSetModelID>
            { id = id; sortingWidth = sortingWidth; }

    member this.Id with get() = this.id

    member this.SortingWidth with get() = this.sortingWidth

    override this.Equals(obj) = 
        match obj with
        | :? MsasF as other -> 
            this.sortingWidth = other.sortingWidth
        | _ -> false

    override this.GetHashCode() = 
        hash (this.sortingWidth)

    interface IEquatable<MsasF> with
        member this.Equals(other) = 
            this.sortingWidth = other.sortingWidth

    member this.MakeSortableArraySet (sortingWidth: int<sortingWidth>) : SortableArraySet =
        let sortableArrays = 
                SortableBoolArray.getAllSortableBoolArrays sortingWidth |> Array.map(SortableArray.Bools)
        SortableArraySet.create ( %this.id |> UMX.tag<sortableSetId>) sortableArrays


module MsasF = ()
 
 